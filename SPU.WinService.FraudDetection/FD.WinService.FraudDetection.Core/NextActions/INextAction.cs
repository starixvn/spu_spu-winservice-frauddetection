// --------------------------------------------------------------------------
// <copyright file="INextAction.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.NextActions
{
    using System.Collections.Generic;
    using Lib.DTO;

    /// <summary>
    ///     BaseAction Class.
    /// </summary>
    public interface INextAction
    {
        IList<SuspiciousInfo> SuspicionList { get; set; }

        IList<DataCapture> DataCaptureList { get; set; }

        void TakeAction();
    }
}