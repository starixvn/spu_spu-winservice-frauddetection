// --------------------------------------------------------------------------
// <copyright file="NextActionFactory.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.NextActions
{
    using System;

    /// <summary>
    ///     ActionFactory Class.
    /// </summary>
    public static class NextActionFactory
    {
        public static INextAction GetNextAction(NextActionType actionType, int targetId, int caseId)
        {
            switch (actionType)
            {
                case NextActionType.AssigningGroup:
                    return new AssigningGroupNextAction(targetId, caseId);

                case NextActionType.SendingEmail:
                    return new SendingEmailNextAction(targetId);

                case NextActionType.CapturingData:
                    return new SavingCapturedData(targetId);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}