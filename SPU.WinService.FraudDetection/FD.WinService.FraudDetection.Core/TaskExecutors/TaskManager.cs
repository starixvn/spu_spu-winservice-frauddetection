// --------------------------------------------------------------------------
// <copyright file="TaskManager.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.Lib.BusinessManager.TaskExecutors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Criteria.Task;
    using DTO;
    using Fanex.Logging;
    using Lib.TaskExecutors;
    using Service.Implement;
    using Service.Interface;

    /// <summary>
    ///     TaskManager Class.
    /// </summary>
    public class TaskManager : ITaskManager
    {
        private readonly ITaskService _taskService;

        public TaskManager()
        {
            TaskExecutors = new List<TaskExecutor>();
            _taskService = new TaskService();
        }

        public TaskManager(ITaskService taskService)
        {
            TaskExecutors = new List<TaskExecutor>();
            _taskService = taskService;
        }

        public IList<TaskExecutor> TaskExecutors { get; set; }

        /// <summary>
        ///     Add and start a task in the thread pool.
        /// </summary>
        /// <param name="taskId">The task to execute.</param>
        public void AddTask(int taskId)
        {
            try
            {
                var task = _taskService.GetTaskForExecuting(new TaskGetForExecutingCriteria()
                {
                    TaskId = taskId
                });

                AddTask(task);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
        }

        public void AddTasksInCase(int caseId)
        {
            var tasks = _taskService.GetTasksByCaseId(new TaskGetByCaseIdCriteria()
            {
                CaseId = caseId
            });

            foreach (var task in tasks)
            {
                AddTask(task);
            }
        }

        public void ClearTasks()
        {
            foreach (var taskExecutor in TaskExecutors)
            {
                taskExecutor.Stop();
            }

            TaskExecutors.Clear();
        }

        public bool Exist(Task task)
        {
            return TaskExecutors.Any(x => x.TaskId == task.TaskId);
        }

        /// <summary>
        ///     Remove a task out the thread pool.
        /// </summary>
        /// <param name="taskId">The task name.</param>
        public void RemoveTask(int taskId)
        {
            try
            {
                var taskExecutor =
                    TaskExecutors.SingleOrDefault(
                        x => x.Task.TaskId == taskId);

                if (taskExecutor == null)
                {
                    return;
                }

                taskExecutor.Stop();

                TaskExecutors.Remove(taskExecutor);
            }
            catch (Exception ex)
            {
                WriteLog(ex);
            }
        }

        public void RemoveTasksInCase(int caseId)
        {
            var taskExecutorsForCase = TaskExecutors.Where(t => t.Task.CaseId == caseId).ToList();

            foreach (var taskExecutor in taskExecutorsForCase)
            {
                taskExecutor.Stop();

                TaskExecutors.Remove(taskExecutor);
            }
        }

        /// <summary>
        ///     Update a task.
        /// </summary>
        /// <param name="taskId">The task to update.</param>
        public void UpdateTask(int taskId)
        {
            RemoveTask(taskId);

            AddTask(taskId);
        }

        protected void WriteLog(Exception exception)
        {
            var message = "\r\nError:" + exception.Message +
                          "\r\nStackTrace: " + exception.StackTrace;

            Logger.Log.Error(message);
        }

        private void AddTask(Task task)
        {
            if (!task.IsActive || Exist(task))
            {
                return;
            }

            var taskExecutor = TaskExecutorFactory.GetTaskExecutor(task);

            TaskExecutors.Add(taskExecutor);

            taskExecutor.Start();
        }
    }
}