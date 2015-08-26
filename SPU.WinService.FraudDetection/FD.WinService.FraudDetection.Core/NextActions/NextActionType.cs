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
        /// <summary>
        /// No Next Action.
        /// </summary>
        None = 0,

        /// <summary>
        /// Assign customers to group.
        /// </summary>
        AssigningGroup = 1,

        /// <summary>
        /// Send email to senior.
        /// </summary>
        SendingEmail = 2
    }
}