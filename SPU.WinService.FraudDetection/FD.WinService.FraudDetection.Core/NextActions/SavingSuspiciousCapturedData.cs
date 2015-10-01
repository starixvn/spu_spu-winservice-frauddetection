// --------------------------------------------------------------------------
// <copyright file="SavingSuspiciousCapturedData.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Aaron.Cao" date="2015.10.01">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.NextActions
{
    using System.Collections.Generic;
    using Fanex.Logging;
    using Lib.Criteria.DataCapture;
    using Lib.DTO;
    using Lib.Service.Implement;

    /// <summary>
    /// Save suspicious info.
    /// </summary>
    public class SavingSuspiciousCapturedData : INextAction
    {
        private readonly DataCaptureService _dataCaptureService;

        public SavingSuspiciousCapturedData()
        {
            _dataCaptureService = new DataCaptureService();
        }

        public IList<SuspiciousInfo> SuspicionList { get; set; }

        public IList<DataCapture> DataCaptureList { get; set; }

        public void TakeAction()
        {
            InsertSuspiciousCapturedData();
        }

        private void InsertSuspiciousCapturedData()
        {
            foreach (var dataCapture in DataCaptureList)
            {
                var suspiciousCapturedDataInsertCriteria = new SuspiciousCapturedDataInsertCriteria
                {
                    DataId = dataCapture.DataId,
                    MatchId = dataCapture.MatchId,
                    NewDate = dataCapture.NewDate,
                    NewValue = dataCapture.NewValue,
                    OldDate = dataCapture.OldDate,
                    OldValue = dataCapture.OldValue
                };

                var result = _dataCaptureService.InsertSuspiciousCapturedData(suspiciousCapturedDataInsertCriteria);

                if (!result.IsSuccessful)
                {
                    Logger.Log.Error("Error UpdateScannedMatch: " + result.Message);
                }
            }
        }
    }
}
