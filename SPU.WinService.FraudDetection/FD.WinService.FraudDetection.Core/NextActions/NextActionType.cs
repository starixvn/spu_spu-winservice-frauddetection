// --------------------------------------------------------------------------
// <copyright file="NextActionType.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.NextActions
{
    public enum NextActionType
    {
        None = 0,
        AssigningGroup = 1,
        SendingEmail = 2,
        CapturingData = 3,
        SendingMessage = 4
    }
}