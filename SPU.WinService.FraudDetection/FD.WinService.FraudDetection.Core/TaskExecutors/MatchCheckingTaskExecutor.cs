// --------------------------------------------------------------------------
// <copyright file="MatchCheckingTaskExecutor.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Aaron.Cao" date="2015.09.29">Created</change>
// </history>

namespace FD.Lib.TaskExecutors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BusinessManager.TaskExecutors;
    using Core.Utilities;
    using DTO;
    using Service.Interface;
    using WinService.FraudDetection.Core.NextActions;
    using Task = DTO.Task;

    public class MatchCheckingTaskExecutor : TaskExecutor
    {
        private readonly List<SuspiciousInfo> _suspicionList;

        private readonly List<Task<IList<SuspiciousInfo>>> _threads;

        private IList<string> _matchIdList;

        public MatchCheckingTaskExecutor(Task task)
            : base(task)
        {
            _matchIdList = new List<string>();
            _suspicionList = new List<SuspiciousInfo>();
            _threads = new List<Task<IList<SuspiciousInfo>>>();
        }

        public MatchCheckingTaskExecutor(Task task, ITaskService taskService)
            : base(task, taskService)
        {
            _matchIdList = new List<string>();
            _suspicionList = new List<SuspiciousInfo>();
            _threads = new List<Task<IList<SuspiciousInfo>>>();
        }

        protected override void ClearOldResult()
        {
            _threads.Clear();
            _matchIdList.Clear();
            _suspicionList.Clear();
        }

        protected override void ExecutePrecondition()
        {
            var preConditionQuery = Task.TaskPreconditions.Replace("$param1$", Task.TaskId.ToString());
            _matchIdList = TaskService.ExecutePrecondition(preConditionQuery).ToList();
        }

        protected override void ExecuteRule()
        {
            FindSuspiciousCustomerOnAllMatches();
        }

        protected override void TakeNextAction()
        {
            if (_suspicionList.Count > 0)
            {
                foreach (var item in NextActionDictionary)
                {
                    Console.WriteLine("<Key, Value> = <" + item.Key + ", " + item.Value + ">");
                    var actionType = Utility.ParseEnum<NextActionType>(item.Key.ToString());
                    var nextAction = NextActionFactory.GetNextAction(actionType, item.Value, Task.CaseId);
                    nextAction.SuspicionList = _suspicionList;
                    nextAction.TakeAction();
                }
            }

            SaveSuspiciousCustomers();
        }

        private void FindSuspiciousCustomerOnAllMatches()
        {
            var coreCount = Environment.ProcessorCount;
            var matchCount = _matchIdList.Count;
            int count = 0;

            for (; count < matchCount; count++)
            {
                if (count >= coreCount)
                {
                    break;
                }

                CreateAndAddNewThread(count);
            }

            int threadsHaveDone = 0;

            while (threadsHaveDone < matchCount)
            {
                int index = System.Threading.Tasks.Task.WaitAny(_threads.ToArray());

                if (_threads[index].Exception == null)
                {
                    _suspicionList.AddRange(_threads[index].Result);
                }

                threadsHaveDone++;
                _threads.RemoveAt(index);

                if (count < matchCount)
                {
                    CreateAndAddNewThread(count);
                    count++;
                }
            }
        }

        private void CreateAndAddNewThread(int count)
        {
            var thread = System.Threading.Tasks.Task.Factory.StartNew(
                (obj) =>
                {
                    var id = (string)obj;
                    return FindSuspiciousCustomerOnEachMatch(id);
                },
            _matchIdList[count]);

            _threads.Add(thread);
        }

        private IList<SuspiciousInfo> FindSuspiciousCustomerOnEachMatch(string matchId)
        {
            var ruleQuery = Task.TaskRules.Replace("$param1$", matchId);
            return TaskService.ExecuteRule(ruleQuery, Task.TaskId, Task.CaseId);
        }

        private void SaveSuspiciousCustomers()
        {
            var saveAction = new SavingSuspicious(_suspicionList, _matchIdList, Task.TaskId);
            saveAction.TakeAction();
        }
    }
}