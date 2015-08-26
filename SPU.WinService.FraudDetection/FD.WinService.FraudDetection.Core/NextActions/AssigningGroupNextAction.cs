// --------------------------------------------------------------------------
// <copyright file="AssigningGroupNextAction.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.NextActions
{
    using System.Collections.Generic;
    using System.Linq;
    using Fanex.Logging;
    using Lib.Core.Utilities;
    using Lib.Criteria.Group;
    using Lib.DTO;
    using Lib.Service.Implement;
    using Lib.Service.Interface;

    /// <summary>
    ///     Assign suspicious customers to a customer group.
    /// </summary>
    public class AssigningGroupNextAction : INextAction
    {
        protected const int ChunkSize = 60;

        public AssigningGroupNextAction(int groupId, int caseId)
        {
            CaseId = caseId;
            GroupId = groupId;
            SuspicionList = new List<SuspiciousInfo>();
            CustomerGroupService = new CustomerGroupService();
        }

        public AssigningGroupNextAction(int groupId, int caseId, ICustomerGroupService groupService)
        {
            CaseId = caseId;
            GroupId = groupId;
            SuspicionList = new List<SuspiciousInfo>();
            CustomerGroupService = groupService;
        }

        public int GroupId { get; set; }

        public int CaseId { get; set; }

        public IList<SuspiciousInfo> SuspicionList { get; set; }

        public ICustomerGroupService CustomerGroupService { get; set; }

        public void TakeAction()
        {
            AssignSuspiciousCustomersToGroup();
        }

        private void AssignSuspiciousCustomersToGroup()
        {
            var custIdList = SuspicionList.Select(t => t.CustId).Distinct().ToList();

            var custIdChunks = custIdList.Chunk(ChunkSize);

            foreach (var custIdChunk in custIdChunks)
            {
                var criteria = new CustomerGroupInsertCriteria
                {
                    CaseId = CaseId,
                    GroupId = GroupId,
                    CustIdList = string.Join(",", custIdChunk)
                };

                var result = CustomerGroupService.InsertCustomerGroup(criteria);

                if (result.IsSuccessful == false)
                {
                    Logger.Log.Error("Error  AssignCustomerGroup : " + result.Message);
                }
            }
        }
    }
}