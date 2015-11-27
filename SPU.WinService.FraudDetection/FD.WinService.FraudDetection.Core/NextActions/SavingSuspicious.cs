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
    using System.Collections.Generic;
    using System.Text;
    using Fanex.Logging;
    using Lib.Core.Utilities;
    using Lib.Criteria.SuspiciousInfo;
    using Lib.DTO;
    using Lib.Service.Implement;
    using Lib.Service.Interface;

    /// <summary>
    /// Save suspicious info.
    /// </summary>
    public class SavingSuspicious
    {
        protected const int ChunkSize = 50;

        private List<SuspiciousInfo> _suspiciousInfoList;

        public SavingSuspicious(List<SuspiciousInfo> suspiciousInfoList)
        {
            _suspiciousInfoList = suspiciousInfoList;
            SuspiciousInfoService = new SuspiciousInfoService();
            SuspiciousCustomerService = new SuspiciousCustomerService();
            CustomerService = new CustomerService();
        }

        public ISuspiciousInfoService SuspiciousInfoService { get; set; }

        public ISuspiciousCustomerService SuspiciousCustomerService { get; set; }

        public ICustomerService CustomerService { get; set; }

        public List<SuspiciousInfo> AssignGroupList
        {
            get { return _suspiciousInfoList; }
            set
            {
                _suspiciousInfoList = value;
                _suspiciousInfoList = AssignGroupList;
            }
        }

        public void TakeAction()
        {
            InsertSuspiciousInfo();
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

        private string CreateXmlParamForInsertingSuspiciousInfo(IEnumerable<SuspiciousInfo> suspiciousInfos)
        {
            var builder = new StringBuilder();
            builder.Append("<Root>");

            foreach (var suspiciousInfo in suspiciousInfos)
            {
                builder.Append("<Row>");
                builder.Append(GetElement("CustId", suspiciousInfo.CustId));
                builder.Append(GetElement("CustName", suspiciousInfo.CustName));
                builder.Append(GetElement("EventDate", suspiciousInfo.EventDate));
                builder.Append(GetElement("MatchId", suspiciousInfo.MatchId));
                builder.Append(GetElement("TransId", suspiciousInfo.TransId));
                builder.Append(GetElement("TaskId", suspiciousInfo.TaskId));
                builder.Append(GetElement("CaseId", suspiciousInfo.CaseId));
                builder.Append(GetElement("ViolationCounter", suspiciousInfo.ViolationCounter));
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