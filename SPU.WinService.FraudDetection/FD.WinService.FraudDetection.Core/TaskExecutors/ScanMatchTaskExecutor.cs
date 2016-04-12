// --------------------------------------------------------------------------
// <copyright file="ScanMatchTaskExecutor.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.Lib.TaskExecutors
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using BusinessManager.TaskExecutors;
    using Core.Utilities;
    using DTO;
    using Fanex.Logging;
    using Service.Interface;
    using WinService.FraudDetection.Core.NextActions;
    using Task = DTO.Task;

    public class ScanMatchTaskExecutor: TaskExecutor
    {
        private readonly List<SuspiciousInfo> _suspicionList;

        private readonly List<Task<IList<SuspiciousInfo>>> _threads;

        private IList<Match> _matchList;

        public ScanMatchTaskExecutor(Task task)
            : base(task)
        {
            _matchList = new List<Match>();
            _suspicionList = new List<SuspiciousInfo>();
            _threads = new List<Task<IList<SuspiciousInfo>>>();
        }

        public ScanMatchTaskExecutor(Task task, ITaskService taskService)
            : base(task, taskService)
        {
            _matchList = new List<Match>();
            _suspicionList = new List<SuspiciousInfo>();
            _threads = new List<Task<IList<SuspiciousInfo>>>();
        }

        protected override void ClearOldResult()
        {
            _threads.Clear();
            _matchList.Clear();
            _suspicionList.Clear();
        }

        protected override void ExecutePrecondition()
        {
            var preConditionQuery = Task.TaskPreconditions.Replace("$param1$", Task.TaskId.ToString(CultureInfo.InvariantCulture));
            _matchList = TaskService.ExecutePrecondition(preConditionQuery).ToList();
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
                    var actionType = Utility.ParseEnum<NextActionType>(item.Key.ToString(CultureInfo.InvariantCulture));
                    var nextAction = NextActionFactory.GetNextAction(actionType, item.Value, Task.CaseId);
                    nextAction.SuspicionList = _suspicionList;
                    nextAction.TakeAction();
                }

                SaveSuspiciousCustomers();
            }

            SaveScannedMatches();
        }

        private void SaveScannedMatches()
        {
            var matchIdList = (from m in _matchList
                               select m.MatchId.ToString(CultureInfo.InvariantCulture)).Distinct().ToList();
            var scannedMatch = new SavingScannedMatch(matchIdList, Task.TaskId);
            scannedMatch.TakeAction();
        }

        private void FindSuspiciousCustomerOnAllMatches()
        {
            var coreCount = Environment.ProcessorCount;
            var matchCount = _matchList.Count;
            var count = 0;

            for (; count < matchCount; count++)
            {
                if (count >= coreCount)
                {
                    break;
                }

                CreateAndAddNewThread(count);
            }

            var threadsHaveDone = 0;

            while (threadsHaveDone < matchCount)
            {
                var index = System.Threading.Tasks.Task.WaitAny(_threads.ToArray());

                if (_threads[index].Exception == null)
                {
                    _suspicionList.AddRange(_threads[index].Result);
                }
                else
                {
                    throw _threads[index].Exception;
                }

                threadsHaveDone++;
                _threads.RemoveAt(index);

                if (count >= matchCount)
                    continue;
                CreateAndAddNewThread(count);
                count++;
            }
        }

        private void CreateAndAddNewThread(int count)
        {
            var thread = System.Threading.Tasks.Task.Factory.StartNew(
                obj =>
                {
                    var id = (Match)obj;
                    return FindSuspiciousCustomerOnEachMatch(id);
                },
            _matchList[count]);

            _threads.Add(thread);
        }

        private IList<SuspiciousInfo> FindSuspiciousCustomerOnEachMatch(Match match)
        {
            var result = new List<SuspiciousInfo>();
            var ruleQuery = string.Empty;

            try
            {
                ruleQuery = Task.TaskRules.Replace("$param1$", match.MatchId.ToString(CultureInfo.InvariantCulture));
                result = TaskService.ExecuteRule(ruleQuery, Task.TaskId, Task.CaseId, match.EventDate);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("MatchId = " + match.MatchId.ToString() + ", ruleQuery = " + ruleQuery);

                throw ex;
            }

            return result;
        }

        private void SaveSuspiciousCustomers()
        {
            var saveAction = new SavingSuspicious(_suspicionList);
            saveAction.TakeAction();
        }
    }
}