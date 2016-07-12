// --------------------------------------------------------------------------
// <copyright file="ScanMatchTaskExecutor.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

using System.Globalization;

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

    public class FindBettingGroupsTaskExecutor : TaskExecutor
    {
        private readonly List<SuspiciousGroupInfo> _suspiciousList;

        private readonly List<Task<IList<SuspiciousGroupInfo>>> _threads;

        private IList<Match> _matchList;

        public FindBettingGroupsTaskExecutor(Task task)
            : base(task)
        {
            _matchList = new List<Match>();
            _suspiciousList = new List<SuspiciousGroupInfo>();
            _threads = new List<Task<IList<SuspiciousGroupInfo>>>();
        }

        public FindBettingGroupsTaskExecutor(Task task, ITaskService taskService)
            : base(task, taskService)
        {
            _matchList = new List<Match>();
            _suspiciousList = new List<SuspiciousGroupInfo>();
            _threads = new List<Task<IList<SuspiciousGroupInfo>>>();
        }

        protected override void ClearOldResult()
        {
            _threads.Clear();
            _matchList.Clear();
            _suspiciousList.Clear();
        }

        protected override void ExecutePrecondition()
        {
            if (Task.HasPrecondition)
            {
                var preConditionQuery = Task.TaskPreconditions.Replace("$param1$", Task.TaskId.ToString(CultureInfo.InvariantCulture));
                _matchList = TaskService.ExecutePrecondition(preConditionQuery).ToList();
            }
        }

        protected override void ExecuteRule()
        {
            if (Task.HasRule)
            {
                FindSuspiciousGroupsOnAllMatches();
            }
        }

        protected override void TakeNextAction()
        {
            if (_suspiciousList.Count > 0)
            {
                if (Task.HasNextAction)
                {
                    foreach (var item in NextActionDictionary)
                    {
                        Console.WriteLine("<Key, Value> = <" + item.Key + ", " + item.Value + ">");
                        var actionType = Utility.ParseEnum<NextActionType>(item.Key.ToString(CultureInfo.InvariantCulture));
                        var nextAction = NextActionFactory.GetNextAction(actionType, item.Value, Task);
                        nextAction.SuspiciousList = ConvertIntoSuspiciousInfo(_suspiciousList);
                        nextAction.TakeAction();
                    }
                }

                SaveSuspiciousGroups();
            }

            if (_matchList != null && _matchList.Count > 0)
            {
                SaveScannedMatches();
            }
        }

        private IList<SuspiciousInfo> ConvertIntoSuspiciousInfo(List<SuspiciousGroupInfo> _suspiciousList)
        {
            return _suspiciousList.Select(suspicious => new SuspiciousInfo
            {
                CaseId = suspicious.CaseId,
                CustId = suspicious.CustId,
                MatchId = suspicious.MatchId,
                TaskId = suspicious.TaskId,
                TransId = suspicious.TransId
            }).ToList();
        }

        private void FindSuspiciousGroupsOnAllMatches()
        {
            var coreCount = Environment.ProcessorCount;
            var matchCount = _matchList.Count;
            int count = 0;

            for (; count < matchCount; count++)
            {
                if (count >= coreCount)
                {
                    break;
                }

                CreateAndAddNewThread(_matchList[count].MatchId);
            }

            int threadsHaveDone = 0;

            while (threadsHaveDone < matchCount)
            {
                int index = System.Threading.Tasks.Task.WaitAny(_threads.ToArray());

                if (_threads[index].Exception == null)
                {
                    _suspiciousList.AddRange(_threads[index].Result);
                }
                else
                {
                    throw _threads[index].Exception;
                }

                threadsHaveDone++;
                _threads.RemoveAt(index);

                if (count < matchCount)
                {
                    CreateAndAddNewThread(_matchList[count].MatchId);
                    count++;
                }
            }
        }

        private void SaveScannedMatches()
        {
            if (_matchList != null)
            {
                var matchIdList = _matchList.Select(m => m.MatchId.ToString()).ToList();
                var scannedMatch = new SavingScannedMatch(matchIdList, Task.TaskId);

                scannedMatch.TakeAction();
            }
        }

        private void CreateAndAddNewThread(int matchId)
        {
            var thread = System.Threading.Tasks.Task.Factory.StartNew(
                (obj) =>
                {
                    var id = (int)obj;
                    var result = FindSuspiciousGroupsOnEachMatch(id);
                    return result;
                },
                matchId);

            _threads.Add(thread);
        }

        private IList<SuspiciousGroupInfo> FindSuspiciousGroupsOnEachMatch(int matchId)
        {
            var ruleQuery = Task.TaskRules.Replace("$param1$", matchId.ToString());
            var result = TaskService.ExecuteRuleForGrouping(ruleQuery, Task.TaskId, Task.CaseId);
            return result;
        }

        private void SaveSuspiciousGroups()
        {
            var saveAction = new SavingSuspiciousGroup(_suspiciousList);
            saveAction.TakeAction();
        }
    }
}