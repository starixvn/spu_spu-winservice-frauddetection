// --------------------------------------------------------------------------
// <copyright file="CountingSuspicousTimesTaskExecutor.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.Lib.TaskExecutors
{
    using System.Collections.Generic;
    using System.Linq;
    using BusinessManager.TaskExecutors;
    using Core.Utilities;
    using Criteria.SuspiciousInfo;
    using DTO;
    using Service.Interface;
    using WinService.FraudDetection.Core.NextActions;

    public class CountingSuspicousTimesTaskExecutor : TaskExecutor
    {
        private IList<string> _customerIdList;

        private IList<SuspiciousCustomer> _suspiciousCustomers;

        public CountingSuspicousTimesTaskExecutor(Task task)
            : base(task)
        {
            _customerIdList = new List<string>();
            _suspiciousCustomers = new List<SuspiciousCustomer>();
        }

        public CountingSuspicousTimesTaskExecutor(Task task, ITaskService taskService)
            : base(task, taskService)
        {
            _customerIdList = new List<string>();
            _suspiciousCustomers = new List<SuspiciousCustomer>();
        }

        protected override void ClearOldResult()
        {
            _customerIdList.Clear();
            _suspiciousCustomers.Clear();
        }

        protected override void ExecutePrecondition()
        {
            if (Task.HasPrecondition)
            {
                var preConditionQuery = Task.TaskPreconditions;
                _customerIdList = TaskService.ExecutePreconditionForCountingSuspiciousTimes(preConditionQuery).ToList();
            }
        }

        protected override void ExecuteRule()
        {
            if (Task.HasRule)
            {
                var ruleQuery = Task.TaskRules;

                if (_customerIdList.Any())
                {
                    var taskRule = Task.TaskRules + " AND susCust.CustId in ($param1$)";
                    ruleQuery = taskRule.Replace("$param1$", string.Join(",", _customerIdList));
                    _suspiciousCustomers = TaskService.GetSuspiciousCustomers(ruleQuery, Task.TaskId);
                }
            }
        }

        protected override void TakeNextAction()
        {
            if (Task.HasNextAction)
            {
                if (_suspiciousCustomers.Count == 0)
                {
                    return;
                }

                var suspiciousCustomerChunks = _suspiciousCustomers.Chunk(ChunkSize);
                var suspciousList = new List<SuspiciousInfo>();

                foreach (var suspiciousCustomerChunk in suspiciousCustomerChunks)
                {
                    var criteria = new SuspiciousHistoryCriteria
                    {
                        CustIdList = string.Join(",", suspiciousCustomerChunk.Select(t => t.CustId)),
                        CaseId = Task.CaseId
                    };

                    suspciousList.AddRange(TaskService.GetSuspiciousCustomerHistory(criteria));
                }

                foreach (var item in NextActionDictionary)
                {
                    var actionType = Utility.ParseEnum<NextActionType>(item.Key.ToString());
                    var nextAction = NextActionFactory.GetNextAction(actionType, item.Value, Task);
                    nextAction.SuspicionList = suspciousList;
                    nextAction.TakeAction();
                }
            }
        }
    }
}