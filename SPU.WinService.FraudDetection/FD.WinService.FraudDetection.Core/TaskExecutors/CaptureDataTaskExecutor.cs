// --------------------------------------------------------------------------
// <copyright file="CaptureDataTaskExecutor.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Aaron.Cao" date="2015.09.29">Created</change>
// </history>

using FD.Lib.Criteria.Common;

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

        private readonly TaskService _taskService;

        private readonly int _taskId;

        private IList<string> _matchIdList;

        private const int TicketChunk = 100000;

        public CaptureDataTaskExecutor(Task task)
            : base(task)
        {
            _matchIdList = new List<string>();
            _dataCaptureList = new List<DataCapture>();
            _threads = new List<Task<IList<DataCapture>>>();
            _scannedMatchesService = new ScannedMatchesService();
            _taskService = new TaskService();
            _taskId = task.TaskId;
        }

        public CaptureDataTaskExecutor(Task task, ITaskService taskService)
            : base(task, taskService)
        {
            _matchIdList = new List<string>();
            _dataCaptureList = new List<DataCapture>();
            _threads = new List<Task<IList<DataCapture>>>();
            _scannedMatchesService = new ScannedMatchesService();
            _taskService = new TaskService();
            _taskId = task.TaskId;
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
            foreach (var item in NextActionDictionary)
            {
                var actionType = Utility.ParseEnum<NextActionType>(item.Key.ToString(CultureInfo.InvariantCulture));
                var nextAction = NextActionFactory.GetNextAction(actionType, item.Value, Task.CaseId);
                nextAction.DataCaptureList = _dataCaptureList;
                nextAction.TakeAction();
            }

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

            var dataTypeList = _taskService.GetAllDataTypeCapture(new DataTypeCaptureGetAllCriteria());

            foreach (var dataType in dataTypeList)
            {
                var ticketChunks = tickets.Chunk(TicketChunk);
                decimal value = 0; 
                
                foreach (var ticketChunk in ticketChunks)
                {
                    value += TaskService.CaptureData(GenerateSqlQuery(ticketChunk, dataType.DataId));
                }
                
                var dataCapture = new DataCapture
                {
                    DataId = dataType.DataId,
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

        private string GenerateSqlQuery(IEnumerable<string> tickets, int dataType)
        {
            var sqlQueryString = "select $data$ as DataCapture FROM bodb02.dbo.bettrans b with(nolock) where b.transid in " + "(" + string.Join(",", tickets.ToArray()) + ")";

            switch (dataType)
            {
                case 1:
                    sqlQueryString = sqlQueryString.Replace("$data$", "SUM(b.stake * (1 -  b.tpositiontaking))");
                    break;

                case 2:
                    sqlQueryString = sqlQueryString.Replace("$data$", "SUM(b.stake * b.spositiontaking)");
                    break;

                case 3:
                    sqlQueryString = sqlQueryString.Replace("$data$", "SUM(b.stake * b.mpositiontaking)");
                    break;

                case 4:
                    sqlQueryString = sqlQueryString.Replace("$data$", "SUM(b.stake * b.apositiontaking)");
                    break;

                case 5:
                    sqlQueryString = sqlQueryString.Replace("$data$", "COUNT(1)");
                    sqlQueryString = sqlQueryString + " and b.statusid in (101, 102, 103, 111, 112, 113)";
                    break;

                case 6:
                    sqlQueryString = sqlQueryString.Replace("$data$", "COUNT(1)");
                    sqlQueryString = sqlQueryString + " and b.statusid in (111, 112, 113)";
                    break;

                case 7:
                    sqlQueryString = sqlQueryString.Replace("$data$", "COUNT(1)");
                    sqlQueryString = sqlQueryString + " and b.statusid in (101, 102, 103)";
                    break;

                default:
                    sqlQueryString = sqlQueryString.Replace("$data$", "COUNT(1)");
                    break;
            }

            return sqlQueryString;
        }
    }
}