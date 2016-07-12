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
    using BusinessManager.TaskExecutors;
    using Core.Utilities;
    using DTO;
    using FD.Lib.Criteria.Common;
    using FD.Lib.Criteria.DataCapture;
    using FD.Lib.Service.Implement;
    using Service.Interface;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using WinService.FraudDetection.Core.NextActions;
    using Task = DTO.Task;

    public class ScanMatchTaskExecutor : TaskExecutor
    {
        private readonly List<SuspiciousInfo> _suspiciousList;

        private readonly List<Task<IList<SuspiciousInfo>>> _threads;

        private readonly DataCaptureService _dataCaptureService;

        private IList<Match> _matchList;

        private IList<Match> _removeMatchList;

        public ScanMatchTaskExecutor(Task task)
            : base(task)
        {
            _matchList = new List<Match>();
            _removeMatchList = new List<Match>();
            _suspiciousList = new List<SuspiciousInfo>();
            _threads = new List<Task<IList<SuspiciousInfo>>>();
            _dataCaptureService = new DataCaptureService();
        }

        public ScanMatchTaskExecutor(Task task, ITaskService taskService)
            : base(task, taskService)
        {
            _matchList = new List<Match>();
            _removeMatchList = new List<Match>();
            _suspiciousList = new List<SuspiciousInfo>();
            _threads = new List<Task<IList<SuspiciousInfo>>>();
            _dataCaptureService = new DataCaptureService();
        }

        protected override void ClearOldResult()
        {
            _threads.Clear();
            _matchList.Clear();
            _suspiciousList.Clear();
            _removeMatchList.Clear();
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
                FindSuspiciousCustomerOnAllMatches();
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
                        var actionType = Utility.ParseEnum<NextActionType>(item.Key.ToString(CultureInfo.InvariantCulture));
                        var nextAction = NextActionFactory.GetNextAction(actionType, item.Value, Task);
                        nextAction.SuspiciousList = _suspiciousList;
                        nextAction.TakeAction();
                    }
                }

                SaveSuspiciousCustomers();
            }

            if (isNeedUpdateDataCapture)
            {
                UpdateDataCaptureTable();
            }

            if (_matchList != null && _matchList.Count > 0)
            {
                foreach (var match in _removeMatchList)
                {
                    _matchList.Remove(match);
                }

                SaveScannedMatches();
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

        private void FindSuspiciousCustomerOnAllMatches()
        {
            var coreCount = Environment.ProcessorCount;
            var matchCount = _matchList.Count;
            var newMatchList = _matchList.Select(m => m).ToList();

            var count = 0;

            for (; count < matchCount; count++)
            {
                if (count >= coreCount)
                {
                    break;
                }

                CreateAndAddNewThread(newMatchList.ElementAt(count));
            }

            var threadsHaveDone = 0;

            while (threadsHaveDone < matchCount)
            {
                var index = System.Threading.Tasks.Task.WaitAny(_threads.ToArray());

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

                if (count >= matchCount)
                    continue;
                CreateAndAddNewThread(newMatchList.ElementAt(count));
                count++;
            }
        }

        private void CreateAndAddNewThread(Match match)
        {
            var thread = System.Threading.Tasks.Task.Factory.StartNew(
                obj =>
                {
                    var id = (Match)obj;
                    return FindSuspiciousCustomerOnEachMatch(id);
                },
            match);

            _threads.Add(thread);
        }

        private IList<SuspiciousInfo> FindSuspiciousCustomerOnEachMatch(Match match)
        {
            var result = new List<SuspiciousInfo>();
            var ruleQuery = string.Empty;
            var shouldBeChecked = true;

            if (isCheckingCompletedMatches)
            {
                shouldBeChecked = TaskService.CheckAllTicketsSettled(match.MatchId, bettypeIds);
            }

            if (shouldBeChecked)
            {
                try
                {
                    ruleQuery = Task.TaskRules.Replace("$param1$", match.MatchId.ToString(CultureInfo.InvariantCulture));
                    result = TaskService.ExecuteRule(ruleQuery, Task.TaskId, Task.CaseId, match.EventDate);
                }
                catch (Exception ex)
                {
                    _removeMatchList.Add(match);
                    result = new List<SuspiciousInfo>();

                    var error = "<b>Error:</b> " + ex.Message +
                              "<br><b>StackTrace:</b> " + ex.StackTrace;
                    var message = "<b>Problem with MatchId = " + match.MatchId.ToString() + "</b><br><br>" + error;

                    SendMailInform(message);
                }
            }
            else
            {
                _removeMatchList.Add(match);
            }

            return result;
        }

        private void SaveSuspiciousCustomers()
        {
            var saveAction = new SavingSuspicious(_suspiciousList);

            saveAction.TakeAction();
        }

        private void UpdateDataCaptureTable()
        {
            var dataTypeListString = GetDataCaptureTypes();

            if (dataTypeListString.Length > 0 && _matchList.Count > 0)
            {
                var dataCaptureUpdateCriteria = new DataCaptureUpdateCriteria
                {
                    DataIdList = GetDataCaptureTypes(),
                    MatchIdList = string.Join(",", _matchList.Select(m => m.MatchId).ToList())
                };

                _dataCaptureService.UpdateCapturedData(dataCaptureUpdateCriteria);
            }
        }

        private string GetDataCaptureTypes()
        {
            var dataCaptureTypes = _dataCaptureService.GetAllDataTypeCapture(new DataTypeCaptureGetAllCriteria());
            var listTypes = new List<int>();

            foreach (var dataCaptureType in dataCaptureTypes)
            {
                if (Task.TaskPreconditionsFormatted.Contains(dataCaptureType.DataName + " <> Has Changed"))
                {
                    listTypes.Add(dataCaptureType.DataId);
                }
            }

            return string.Join(",", listTypes);
        }
    }
}