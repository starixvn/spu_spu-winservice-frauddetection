// --------------------------------------------------------------------------
// <copyright file="CompareDataTaskExecutor.cs" company="Nexcel Solutions Vietnam">
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
    using BusinessManager.TaskExecutors;
    using Core.Utilities;
    using Criteria.DataCapture;
    using Criteria.ScannedMatch;
    using DTO;
    using Fanex.Logging;
    using Service.Implement;
    using Service.Interface;
    using WinService.FraudDetection.Core.NextActions;
    using Task = DTO.Task;

    public class CompareDataTaskExecutor : TaskExecutor
    {
        private List<DataCapture> _suspiciousDataCaptureList;

        private IList<Match> _matchList;

        private IList<string> _notComparedMatchIdList;

        private readonly DataCaptureService _dataCaptureService;

        private readonly ScannedMatchesService _scannedMatchesService;

        private readonly int _taskId;

        private readonly Task _task;

        public CompareDataTaskExecutor(Task task)
            : base(task)
        {
            _matchList = new List<Match>();
            _notComparedMatchIdList = new List<string>();
            _suspiciousDataCaptureList = new List<DataCapture>();
            _dataCaptureService = new DataCaptureService();
            _scannedMatchesService = new ScannedMatchesService();
            _taskId = task.TaskId;
            _task = task;
        }

        public CompareDataTaskExecutor(Task task, ITaskService taskService)
            : base(task, taskService)
        {
            _matchList = new List<Match>();
            _notComparedMatchIdList = new List<string>();
            _suspiciousDataCaptureList = new List<DataCapture>();
            _dataCaptureService = new DataCaptureService();
            _scannedMatchesService = new ScannedMatchesService();
            _taskId = task.TaskId;
            _task = task;
        }

        protected override void ClearOldResult()
        {
            _matchList.Clear();
            _notComparedMatchIdList.Clear();
            _suspiciousDataCaptureList.Clear();
        }

        protected override void ExecutePrecondition()
        {
            var preConditionQuery = Task.TaskPreconditions.Replace("$param1$", Task.TaskId.ToString(CultureInfo.InvariantCulture));
            _matchList = TaskService.ExecutePrecondition(preConditionQuery).ToList();
        }

        protected override void ExecuteRule()
        {
            GetAllNotComparedMatches();

            FindSuspiciousMatches();
        }

        protected override void TakeNextAction()
        {
            if (_suspiciousDataCaptureList.Count > 0)
            {
                var saveAction = new SavingSuspiciousCapturedData
                {
                    DataCaptureList = _suspiciousDataCaptureList
                };

                saveAction.TakeAction();
            }

            UpdateDataCaptureTable();

            InsertScannedMatch();
        }

        private void GetAllNotComparedMatches()
        {
            if (_matchList.Count > 0)
            {
                var matchIdList = (from m in _matchList select m.MatchId.ToString(CultureInfo.InvariantCulture)).Distinct().ToList();
                _notComparedMatchIdList = _dataCaptureService.GetAllNotComparedMacthes(new DataCaptureGetAllNotComparedMatchesCriteria
                {
                    DataIdList = GetDataCaptureTypes(),
                    MatchIdList = string.Join(",", matchIdList)

                }).ToList();
            }
        }

        private void FindSuspiciousMatches()
        {
            if (_notComparedMatchIdList.Count > 0)
            {
                var matchList = string.Join(",", _notComparedMatchIdList.ToArray());
                var ruleQuery = Task.TaskRules.Replace("$param1$", matchList);

                _suspiciousDataCaptureList = TaskService.ExecuteRuleForComparingData(ruleQuery);
            }
        }

        private void InsertScannedMatch()
        {
            var matchChunks = _notComparedMatchIdList.Chunk(ChunkSize);

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

        private void UpdateDataCaptureTable()
        {
            var dataCaptureUpdateCriteria = new DataCaptureUpdateCriteria
            {
                DataIdList = GetDataCaptureTypes(),
                MatchIdList = string.Join(",", _notComparedMatchIdList)
            };

            _dataCaptureService.UpdateCapturedData(dataCaptureUpdateCriteria);
        }

        private string GetDataCaptureTypes()
        {
            var dataCaptureTypes = _dataCaptureService.GetAllDataTypeCapture(new DataTypeCaptureGetAllCriteria());
            var listTypes = new List<int>();

            foreach (var dataCaptureType in dataCaptureTypes)
            {
                if (_task.TaskRulesFormatted.Contains(dataCaptureType.DataName + " <> Has Changed From Latest Capture"))
                {
                    listTypes.Add(dataCaptureType.DataId);
                }
            }

            return string.Join(",", listTypes);
        }
    }
}