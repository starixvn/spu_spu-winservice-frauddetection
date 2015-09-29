// --------------------------------------------------------------------------
// <copyright file="CaptureDataTaskExecutor.cs" company="Nexcel Solutions Vietnam">
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
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using BusinessManager.TaskExecutors;
    using Core.Utilities;
    using Criteria.ScannedMatch;
    using DTO;
    using Fanex.Logging;
    using Service.Implement;
    using Service.Interface;
    using WinService.FraudDetection.Core.NextActions;
    using Task = DTO.Task;

    public class CaptureDataTaskExecutor : TaskExecutor
    {
        private readonly List<DataCapture> _dataCaptureList;

        private readonly List<Task<IList<DataCapture>>> _threads;

        private readonly ScannedMatchesService _scannedMatchesService;

        private readonly int _taskId;

        private readonly string _dataTypeList;

        private IList<string> _matchIdList;

        public CaptureDataTaskExecutor(Task task)
            : base(task)
        {
            _matchIdList = new List<string>();
            _dataCaptureList = new List<DataCapture>();
            _threads = new List<Task<IList<DataCapture>>>();
            _scannedMatchesService = new ScannedMatchesService();
            _taskId = task.TaskId;
            _dataTypeList = task.NextActionTargetIdList;
        }

        public CaptureDataTaskExecutor(Task task, ITaskService taskService)
            : base(task, taskService)
        {
            _matchIdList = new List<string>();
            _dataCaptureList = new List<DataCapture>();
            _threads = new List<Task<IList<DataCapture>>>();
            _scannedMatchesService = new ScannedMatchesService();
            _taskId = task.TaskId;
            _dataTypeList = task.NextActionTargetIdList;
        }

        protected override void ClearOldResult()
        {
            _threads.Clear();
            _matchIdList.Clear();
            _dataCaptureList.Clear();
        }

        protected override void ExecutePrecondition()
        {
            var preConditionQuery = Task.TaskPreconditions.Replace("$param1$", Task.TaskId.ToString(CultureInfo.InvariantCulture));
            _matchIdList = TaskService.ExecutePrecondition(preConditionQuery).ToList();
        }

        protected override void ExecuteRule()
        {
            FindTicketsOnAllMatches();
        }

        protected override void TakeNextAction()
        {
            SaveCapturedData();

            InsertScannedMatch();
        }

        private void FindTicketsOnAllMatches()
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
                    _dataCaptureList.AddRange(_threads[index].Result);
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
                    return CaptureDataOnEachMatch(id);
                },
            _matchIdList[count]);

            _threads.Add(thread);
        }

        private IList<DataCapture> CaptureDataOnEachMatch(string matchId)
        {
            var ruleQuery = Task.TaskRules.Replace("$param1$", matchId);
            var tickets = TaskService.ExecuteRuleForCapturingData(ruleQuery);
            var result = new List<DataCapture>();

            var dataTypeList = _dataTypeList.Split(',');

            foreach (var dataType in dataTypeList)
            {
                var value = TaskService.CaptureData(tickets, int.Parse(dataType));
                var dataCapture = new DataCapture
                {
                    DataId = int.Parse(dataType),
                    MatchId = int.Parse(matchId),
                    NewValue = value,
                    OldValue = value,
                    NewDate = DateTime.Now,
                    OldDate = DateTime.Now
                };

                result.Add(dataCapture);
            }

            return result;
        }

        private void SaveCapturedData()
        {
            var saveAction = new SavingCapturedData {DataCaptureList = _dataCaptureList};
            
            saveAction.TakeAction();
        }

        private void InsertScannedMatch()
        {
            var matchChunks = _matchIdList.Chunk(ChunkSize);

            foreach (var matchChunk in matchChunks)
            {
                var criteria = new MatchInsertCriteria
                {
                    MatchIdList = string.Join(",", matchChunk),
                    TaskId = _taskId,
                    ScannedDate = DateTime.Now
                };

                var result = _scannedMatchesService.InsertScannedMatch(criteria);

                if (result.IsSuccessful == false)
                {
                    Logger.Log.Error("Error UpdateScannedMatch: " + result.Message);
                }
            }
        }
    }
}