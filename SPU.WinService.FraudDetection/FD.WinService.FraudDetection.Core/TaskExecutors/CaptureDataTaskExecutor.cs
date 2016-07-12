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
        private const int TicketChunk = 100000;

        private readonly List<DataCapture> _dataCaptureList;

        private readonly List<Task<IList<DataCapture>>> _threads;

        private readonly ScannedMatchesService _scannedMatchesService;

        private readonly DataCaptureService _dataCaptureService;

        private readonly int _taskId;

        private IList<Match> _matchList;

        private IList<Match> _removeMatchList;

        public CaptureDataTaskExecutor(Task task)
            : base(task)
        {
            _matchList = new List<Match>();
            _removeMatchList = new List<Match>();
            _dataCaptureList = new List<DataCapture>();
            _threads = new List<Task<IList<DataCapture>>>();
            _scannedMatchesService = new ScannedMatchesService();
            _dataCaptureService = new DataCaptureService();
            _taskId = task.TaskId;
            Task = task;
        }

        public CaptureDataTaskExecutor(Task task, ITaskService taskService)
            : base(task, taskService)
        {
            _matchList = new List<Match>();
            _removeMatchList = new List<Match>();
            _dataCaptureList = new List<DataCapture>();
            _threads = new List<Task<IList<DataCapture>>>();
            _scannedMatchesService = new ScannedMatchesService();
            _dataCaptureService = new DataCaptureService();
            _taskId = task.TaskId;
            Task = task;
        }

        protected override void ClearOldResult()
        {
            _threads.Clear();
            _matchList.Clear();
            _dataCaptureList.Clear();
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
                CaptureDataByTicketsOnAllMatches();
            }
            else if (_matchList.Count > 0)
            {
                CaptureDataByMatchOnAllMatches();
            }
        }

        protected override void TakeNextAction()
        {
            if (Task.HasNextAction)
            {
                foreach (var item in NextActionDictionary)
                {
                    var actionType = Utility.ParseEnum<NextActionType>(item.Key.ToString(CultureInfo.InvariantCulture));
                    var nextAction = NextActionFactory.GetNextAction(actionType, item.Value, Task);
                    nextAction.DataCaptureList = _dataCaptureList;
                    nextAction.TakeAction();
                }
            }

            if (_matchList != null && _matchList.Count > 0)
            {
                foreach (var match in _removeMatchList)
                {
                    _matchList.Remove(match);
                }

                InsertScannedMatch();
            }
        }

        private void CaptureDataByMatchOnAllMatches()
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

                var thread = System.Threading.Tasks.Task.Factory.StartNew(
                    obj =>
                    {
                        var id = (Match)obj;

                        return CaptureDataByMatch(id);
                    },
                newMatchList.ElementAt(count));

                _threads.Add(thread);
            }

            var threadsHaveDone = 0;

            while (threadsHaveDone < matchCount)
            {
                var index = System.Threading.Tasks.Task.WaitAny(_threads.ToArray());

                if (_threads[index].Exception == null)
                {
                    _dataCaptureList.AddRange(_threads[index].Result);
                }
                else
                {
                    throw _threads[index].Exception;
                }

                threadsHaveDone++;
                _threads.RemoveAt(index);

                if (count >= matchCount)
                    continue;

                var thread = System.Threading.Tasks.Task.Factory.StartNew(
                    obj =>
                    {
                        var id = (Match)obj;

                        return CaptureDataByMatch(id);
                    },
                newMatchList.ElementAt(count));

                _threads.Add(thread);

                count++;
            }
        }

        private void CaptureDataByTicketsOnAllMatches()
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

                var thread = System.Threading.Tasks.Task.Factory.StartNew(
                    obj =>
                    {
                        var id = (Match)obj;

                        return CaptureDataByTicketsOnEachMatch(id);
                    },
                newMatchList.ElementAt(count));

                _threads.Add(thread);
            }

            var threadsHaveDone = 0;

            while (threadsHaveDone < matchCount)
            {
                var index = System.Threading.Tasks.Task.WaitAny(_threads.ToArray());

                if (_threads[index].Exception == null)
                {
                    _dataCaptureList.AddRange(_threads[index].Result);
                }
                else
                {
                    throw _threads[index].Exception;
                }

                threadsHaveDone++;
                _threads.RemoveAt(index);

                if (count >= matchCount) continue;

                var thread = System.Threading.Tasks.Task.Factory.StartNew(
                    obj =>
                    {
                        var id = (Match)obj;

                        return CaptureDataByTicketsOnEachMatch(id);
                    },
                newMatchList.ElementAt(count));

                _threads.Add(thread);

                count++;
            }
        }

        private IList<DataCapture> CaptureDataByTicketsOnEachMatch(Match match)
        {
            var result = new List<DataCapture>();
            var shouldBeChecked = true;

            if (isCheckingCompletedMatches)
            {
                shouldBeChecked = TaskService.CheckAllTicketsSettled(match.MatchId, bettypeIds);
            }

            if (shouldBeChecked || isCheckingRunningMatches)
            {
                try
                {
                    var ruleQuery = Task.TaskRules.Replace("$param1$", match.MatchId.ToString(CultureInfo.InvariantCulture));
                    var tickets = TaskService.ExecuteRuleForCapturingData(ruleQuery);

                    foreach (var item in NextActionDictionary)
                    {
                        var ticketChunks = tickets.Chunk(TicketChunk);
                        decimal value = 0;

                        foreach (var ticketChunk in ticketChunks)
                        {
                            value += TaskService.CaptureData(GenerateSqlQueryByTickets(match.MatchId, ticketChunk, item.Value));
                        }

                        var dataCapture = new DataCapture
                        {
                            DataId = item.Value,
                            MatchId = match.MatchId,
                            NewValue = value,
                            OldValue = value,
                            NewDate = DateTime.Now,
                            OldDate = DateTime.Now
                        };

                        result.Add(dataCapture);
                    }
                }
                catch (Exception ex)
                {
                    _removeMatchList.Add(match);
                    result = new List<DataCapture>();

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

        private IList<DataCapture> CaptureDataByMatch(Match match)
        {
            var result = new List<DataCapture>();
            var shouldBeChecked = true;

            if (isCheckingCompletedMatches)
            {
                shouldBeChecked = TaskService.CheckAllTicketsSettled(match.MatchId, bettypeIds);
            }

            if (shouldBeChecked || isCheckingRunningMatches)
            {
                try
                {
                    foreach (var item in NextActionDictionary)
                    {
                        var value = TaskService.CaptureData(GenerateSqlQueryByMatch(match.MatchId, item.Value));

                        var dataCapture = new DataCapture
                        {
                            DataId = item.Value,
                            MatchId = match.MatchId,
                            NewValue = value,
                            OldValue = value,
                            NewDate = DateTime.Now,
                            OldDate = DateTime.Now
                        };

                        result.Add(dataCapture);
                    }
                }
                catch (Exception ex)
                {
                    _removeMatchList.Add(match);
                    result = new List<DataCapture>();

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

        private void InsertScannedMatch()
        {
            if (_matchList != null)
            {
                var matchIdList = _matchList.Select(m => m.MatchId);
                var matchChunks = matchIdList.Chunk(ChunkSize);

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

        private string GenerateSqlQueryByTickets(int matchId, IEnumerable<string> tickets, int dataType)
        {
            var sqlQueryString =
                "select $data$ as DataCapture FROM bodb02.dbo.bettrans b with(nolock) where b.transid in " + "(" + string.Join(",", tickets.ToArray()) + ")";

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

                case 8:
                    sqlQueryString =
                        "select ISNULL((m.livehomescore * 1000 + m.liveawayscore),0) as DataCapture FROM bodb02.dbo.match m with(nolock) where m.matchid in " + "(" + matchId.ToString() + ")";
                    break;

                default:
                    sqlQueryString = sqlQueryString.Replace("$data$", "COUNT(1)");
                    break;
            }

            return sqlQueryString;
        }

        private string GenerateSqlQueryByMatch(int matchId, int dataType)
        {
            var sqlQueryString = "select $data$ as DataCapture FROM bodb02.dbo.bettrans b with(nolock) where b.matchid in " + "(" + matchId + ")";

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

                case 8:
                    sqlQueryString =
                        "select ISNULL((m.livehomescore * 1000 + m.liveawayscore),0) as DataCapture FROM bodb02.dbo.match m with(nolock) where m.matchid in " + "(" + matchId.ToString() + ")";
                    break;

                default:
                    sqlQueryString = sqlQueryString.Replace("$data$", "COUNT(1)");
                    break;
            }

            return sqlQueryString;
        }
    }
}