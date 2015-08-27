// --------------------------------------------------------------------------
// <copyright file="SavingSuspicious.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.NextActions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Fanex.Logging;
    using Lib.Core.Utilities;
    using Lib.Criteria.ScannedMatch;
    using Lib.Criteria.SuspiciousInfo;
    using Lib.Criteria.SuspiciousStatistic;
    using Lib.DTO;
    using Lib.Service.Implement;
    using Lib.Service.Interface;

    /// <summary>
    /// Save suspicious info.
    /// </summary>
    public class SavingSuspicious
    {
        protected const int ChunkSize = 50;

        private readonly IList<string> _matchIdList;

        private readonly int _taskId;

        private List<SuspiciousInfo> _suspiciousInfoList;

        public SavingSuspicious(List<SuspiciousInfo> suspiciousInfoList, IList<string> matchIdList, int taskId)
        {
            _suspiciousInfoList = suspiciousInfoList;
            _matchIdList = matchIdList;
            _taskId = taskId;

            ScannedMatchesService = new ScannedMatchesService();
            SuspiciousInfoService = new SuspiciousInfoService();
            SuspiciousCustomerService = new SuspiciousCustomerService();
            CustomerService = new CustomerService();
        }

        public IScannedMatchesService ScannedMatchesService { get; set; }

        public ISuspiciousInfoService SuspiciousInfoService { get; set; }

        public ISuspiciousCustomerService SuspiciousCustomerService { get; set; }

        public ICustomerService CustomerService { get; set; }

        public List<SuspiciousInfo> AssignGroupList
        {
            get { return _suspiciousInfoList; }
            set { _suspiciousInfoList = AssignGroupList; }
        }

        public void TakeAction()
        {
            if (_suspiciousInfoList.Count > 0)
            {
                InsertSuspiciousCustomers();
                InsertSuspiciousInfo();
            }

            InsertScannedMatch();
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

                var result = ScannedMatchesService.InsertScannedMatch(criteria);

                if (result.IsSuccessful == false)
                {
                    Logger.Log.Error("Error UpdateScannedMatch: " + result.Message);
                }
            }
        }

        private void InsertSuspiciousInfo()
        {
            var suspiciousChunks = _suspiciousInfoList.Chunk(ChunkSize);

            foreach (var suspiciousChunk in suspiciousChunks)
            {
                var criteria = new SuspiciousInfoInsertCriteria
                {
                    SuspiciousDoc = CreateXmlParamForInsertingSuspiciousInfo(suspiciousChunk)
                };

                var result = SuspiciousInfoService.InsertSuspiciousInfo(criteria);

                if (result.IsSuccessful == false)
                {
                    Logger.Log.Error("Error InsertSuspiciousInfo: " + result.Message);
                }
            }
        }

        private void InsertSuspiciousCustomers()
        {
            var newSuspiciousCustomers = FindNewSuspiciousCustomers();

            if (newSuspiciousCustomers.Count == 0)
            {
                return;
            }

            var newSuspiciousCustomerChunks = newSuspiciousCustomers.Chunk(ChunkSize);

            foreach (var newSuspiciousCustomer in newSuspiciousCustomerChunks)
            {

                InsertNewSuspiciousCustomers(newSuspiciousCustomer);
            }
        }

        private void InsertNewSuspiciousCustomers(IEnumerable<int> suspiciousCustomers)
        {
            var customers = CustomerService.GetCustormersByCustIds(new CustomerSelectCriteria()
            {
                ListCustIds = string.Join(",", suspiciousCustomers)
            });

            SuspiciousCustomerService.InsertSuspiciousCustomer(new SuspiciousCustomerInsertCriteria()
            {
                SuspiciousDoc = CreateXmlParamForInsertingSuspiciousCustomers(customers)
            });
        }

        private IList<int> FindNewSuspiciousCustomers()
        {
            var custIds = _suspiciousInfoList.Select(t => t.CustId).Distinct().ToList();

            var custIdChunks = custIds.Chunk(ChunkSize);
            var newSuspiciousCustomers = new List<int>();

            foreach (var custIdChunk in custIdChunks)
            {
                var custStr = string.Join(",", custIdChunk);

                var temp = SuspiciousCustomerService.GetNewSuspiciousCustomers(new NewSuspiciousCustomersGetCriteria
                {
                    SuspiciousCustomers = custStr
                });

                newSuspiciousCustomers.AddRange(temp);
            }

            return newSuspiciousCustomers;
        }

        private string CreateXmlParamForInsertingSuspiciousCustomers(IEnumerable<Customer> suspiciousCustomers)
        {
            var builder = new StringBuilder();
            builder.Append("<Root>");

            foreach (var suspiciousCustomer in suspiciousCustomers)
            {
                builder.Append("<Row>");
                builder.Append(GetElement("CustId", suspiciousCustomer.CustId));
                builder.Append(GetElement("CustName", suspiciousCustomer.CustName));

                builder.Append("</Row>");
            }

            builder.Append("</Root>");

            return builder.ToString();
        }

        private string CreateXmlParamForInsertingSuspiciousInfo(IEnumerable<SuspiciousInfo> suspiciousInfos)
        {
            var builder = new StringBuilder();
            builder.Append("<Root>");

            foreach (var suspiciousInfo in suspiciousInfos)
            {
                builder.Append("<Row>");
                builder.Append(GetElement("CustId", suspiciousInfo.CustId));
                builder.Append(GetElement("MatchId", suspiciousInfo.MatchId));
                builder.Append(GetElement("TransId", suspiciousInfo.TransId));
                builder.Append(GetElement("TaskId", suspiciousInfo.TaskId));
                builder.Append(GetElement("CaseId", suspiciousInfo.CaseId));
                builder.Append("</Row>");
            }

            builder.Append("</Root>");

            return builder.ToString();
        }

        private string GetElement(string tagName, object value)
        {
            return string.Format("<{0}>{1}</{0}>", tagName, value);
        }
    }
}
