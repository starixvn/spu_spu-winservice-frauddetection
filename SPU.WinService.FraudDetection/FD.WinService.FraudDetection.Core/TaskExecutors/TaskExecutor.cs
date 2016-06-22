// --------------------------------------------------------------------------
// <copyright file="TaskExecutor.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Thai.Tran" date="2015.03.25">Created</change>
//   <change who="Long.Luu" date="2015.04.01">Modified: TaskInfo</change>
// </history>

namespace FD.Lib.BusinessManager.TaskExecutors
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Timers;
    using Criteria.Task;
    using Fanex.Logging;
    using Fanex.MailDelivery.Client.Handlers;
    using Lib.TaskExecutors;
    using Service.Implement;
    using Service.Interface;
    using Sunplus.UMControllerLibrary;
    using WinService.FraudDetection.Core.TaskExecutors;
    using Task = DTO.Task;
    using FD.Lib.Criteria.Config;

    /// <summary>
    ///     Task Class.
    /// </summary>
    public abstract class TaskExecutor : ITaskExecutor
    {
        protected const int ChunkSize = 60;

        protected SortedDictionary<int, int> NextActionDictionary;

        protected UMBusiness MaintenanceService;

        protected bool IsCheckingCompletedMatches;

        protected bool IsCheckingRunningMatches;

        protected bool IsNeedUpdateDataCapture;

        protected string BettypeIds;

        #region Constructor

        protected TaskExecutor(Task task)
        {
            Task = task;
            TaskService = new TaskService();
            MaintenanceService = new UMBusiness();
            IsRunning = false;

            if (Task.HasNextAction)
            {
                GetNextActionDictionary();
            }

            if (Task.HasPrecondition)
            {
                IsCheckingCompletedMatches = task.TaskPreconditions.ToLower().Contains("completed");

                IsCheckingRunningMatches = task.TaskPreconditions.ToLower().Contains("running");

                IsNeedUpdateDataCapture = Task.TaskPreconditionsFormatted.ToLower().Contains("has changed");
            }

            if (Task.HasRule)
            {
                BettypeIds = GetBettypeIdList(Task.TaskRulesOriginal);
            }
        }

        private string GetBettypeIdList(string taskRules)
        {
            var result = new List<string>();

            var rules = taskRules.Split('@');

            var configBettypeId = new ConfigurationsService().
                GetConfigs(new ConfigGetAllCriteria()).
                Where(t => t.ConfigName == "Bet Type").
                FirstOrDefault().ConfigId;

            foreach (var rule in rules)
            {
                if (int.Parse(rule.Split('#')[1]) == configBettypeId)
                {
                    result.Add(rule.Split('#')[3].Split(',')[0]);
                }
            }

            return string.Join(",", result);
        }

        protected TaskExecutor(Task task, ITaskService taskService)
        {
            MaintenanceService = new UMBusiness();
            Task = task;
            TaskService = taskService;
            IsRunning = false;

            if (Task.HasNextAction)
            {
                GetNextActionDictionary();
            }

            if (Task.HasPrecondition)
            {
                IsCheckingCompletedMatches = task.TaskPreconditions.ToLower().Contains("completed");
            }

            if (Task.HasRule)
            {
                BettypeIds = GetBettypeIdList(Task.TaskRulesOriginal);
            }
        }

        #endregion Constructor

        #region Properties

        public Task Task
        {
            get;
            set;
        }

        public ITaskService TaskService
        {
            get;
            set;
        }

        public Timer Timer
        {
            get;
            set;
        }

        public int TaskId
        {
            get
            {
                return Task.TaskId;
            }
        }

        private bool IsRunning
        {
            get;
            set;
        }

        private bool IsStoppedManually
        {
            get;
            set;
        }

        #endregion Properties

        #region Execute task

        public void Start()
        {
            Timer = new Timer
            {
                Interval = Task.Timer,
                AutoReset = true,
                Enabled = true
            };

            Timer.Elapsed += PreExecute;

            Timer.Start();

            // Execute the task immediately.
            PreExecute(null, null);
        }

        public virtual void Stop()
        {
            IsStoppedManually = true;
            Timer.Elapsed -= PreExecute;
            Timer.Stop();
            Timer.Close();
            Timer.Dispose();
        }

        public virtual void PreExecute(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (IsRunning || IsStoppedManually)
            {
                return;
            }

#if DEBUG
            var stopwatch = new Stopwatch();
            stopwatch.Start();
#endif

            IsRunning = true;

            Execute();

            IsRunning = false;

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("{0} - {1} - {2} spent {3} running", Task.CaseId, Task.TaskId, Task.TaskName, stopwatch.Elapsed.ToString());
#endif
        }

        public virtual void Execute()
        {
            try
            {
                var isUnderMaintenance = MaintenanceService.isUnderMaintainence(Sites.AccountSite, ConfigurationManager.AppSettings["FDUrl"]);

                if (!isUnderMaintenance)
                {
                    ClearOldResult();

#if DEBUG
                    Console.WriteLine("{0} - {1} - {2} is running", Task.CaseId, Task.TaskId, Task.TaskName);
#endif
                    ExecutePrecondition();

                    ExecuteRule();

                    TakeNextAction();

                    TriggerOtherTasks();
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
        }

        protected abstract void ClearOldResult();

        protected abstract void ExecutePrecondition();

        protected abstract void ExecuteRule();

        protected abstract void TakeNextAction();

        protected void WriteLog(Exception exception)
        {
            var message = "\r\nError: " + exception.Message +
                          "\r\nStackTrace: " + exception.StackTrace;

            SendMailInform(message);
            Logger.Log.Error(message);
        }

        private void GetNextActionDictionary()
        {
            var nextActionIdList = Task.NextActionIdList.Split(',');
            var nextActionTargetIdList = Task.NextActionTargetIdList.Split(',');
            NextActionDictionary = new SortedDictionary<int, int>(new KeyComparer());
            var nextActionCount = nextActionIdList.GetLength(0);

            for (var i = 0; i < nextActionCount; i++)
            {
                NextActionDictionary.Add(int.Parse(nextActionIdList[i]), int.Parse(nextActionTargetIdList[i]));
            }
        }

        private void TriggerOtherTasks()
        {
            var tasks = TaskService.GetTaskForTriggering(new TaskGetForTriggeringCriteria
            {
                TaskId = TaskId
            });

            var taskList = tasks as Task[] ?? tasks.ToArray();
            var threads = new System.Threading.Tasks.Task[taskList.Count()];
            var index = 0;

            foreach (var task in taskList)
            {
                var executingTask = task;
                threads[index] = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        var taskExecutor = TaskExecutorFactory.GetTaskExecutor(executingTask);
                        taskExecutor.Execute();
                    });
                index++;
            }

            System.Threading.Tasks.Task.WaitAll(threads);
        }

        protected void SendMailInform(string message)
        {
            var newMail = new Fanex.MailDelivery.Client.Models.Mail
            {
                Subject = "SPU FS Error",
                Title = "SPU FS Error",
                Body = message,
                MailingListId = int.Parse(ConfigurationManager.AppSettings["MailingListId"])
            };

            MailClientHandler.SendMail(newMail);
        }

        #endregion Execute task
    }

    public class KeyComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            var result = x.CompareTo(y);

            return result == 0 ? 1 : result;
        }
    }
}