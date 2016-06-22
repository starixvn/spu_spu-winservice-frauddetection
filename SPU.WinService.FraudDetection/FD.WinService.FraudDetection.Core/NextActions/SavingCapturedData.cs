// --------------------------------------------------------------------------
// <copyright file="SavingCapturedData.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Aaron.Cao" date="2015.09.29">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.NextActions
{
    using System.Collections.Generic;
    using Fanex.Logging;
    using Lib.Criteria.DataCapture;
    using Lib.DTO;
    using Lib.Service.Implement;
    using System.Text;

    /// <summary>
    /// Save suspicious info.
    /// </summary>
    public class SavingCapturedData : INextAction
    {
        private readonly DataCaptureService _dataCaptureService;

        private readonly int _dataType;

        public SavingCapturedData(int dataType)
        {
            _dataCaptureService = new DataCaptureService();
            _dataType = dataType;
        }

        public IList<SuspiciousInfo> SuspicionList { get; set; }

        public IList<DataCapture> DataCaptureList { get; set; }

        public void TakeAction()
        {
            InsertCapturedData();
        }

        private void InsertCapturedData()
        {
            var dataCaptureList = new List<DataCapture>();

            foreach (var dataCapture in DataCaptureList)
            {
                if (_dataType == dataCapture.DataId)
                {
                    dataCaptureList.Add(dataCapture);
                }
            }

            if(dataCaptureList.Count > 0)
            {
                var criteria = new DataCaptureListInsertCriteria
                {
                    DataCaptureDoc = CreateXmlParamForInsertingDataCapture(dataCaptureList)
                };

                var result = _dataCaptureService.InsertCapturedDataList(criteria);

                if (!result.IsSuccessful)
                {
                    Logger.Log.Error("Error Saving Data Capture: " + result.Message);
                }
            }
        }

        private string CreateXmlParamForInsertingDataCapture(IEnumerable<DataCapture> dataCaptureList)
        {
            var builder = new StringBuilder();
            builder.Append("<Root>");

            foreach (var data in dataCaptureList)
            {
                builder.Append("<Row>");
                builder.Append(GetElement("DataId", data.DataId));
                builder.Append(GetElement("MatchId", data.MatchId));
                builder.Append(GetElement("NewValue", data.NewValue));
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
