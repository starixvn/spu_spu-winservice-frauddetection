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

    /// <summary>
    /// Save suspicious info.
    /// </summary>
    public class SavingCapturedData : INextAction
    {

        private readonly DataCaptureService _dataCaptureService;

        public SavingCapturedData()
        {
            _dataCaptureService = new DataCaptureService();
        }

        public IList<SuspiciousInfo> SuspicionList { get; set; }

        public IList<DataCapture> DataCaptureList { get; set; }

        public void TakeAction()
        {
            InsertCapturedData();
        }

        private void InsertCapturedData()
        {
            foreach (var dataCapture in DataCaptureList)
            {
                var criteria = new DataCaptureInsertCriteria
                {
                    DataId = dataCapture.DataId,
                    MatchId = dataCapture.MatchId,
                    NewDate = dataCapture.NewDate,
                    NewValue = dataCapture.NewValue,
                    OldDate = dataCapture.OldDate,
                    OldValue = dataCapture.OldValue
                };

                var result = _dataCaptureService.InsertCapturedData(criteria);

                if (!result.IsSuccessful)
                {
                    Logger.Log.Error("Error UpdateScannedMatch: " + result.Message);
                }
            }
        }
    }
}
