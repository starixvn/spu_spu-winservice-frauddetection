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
    using FD.Lib.DTO;
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     ActionFactory Class.
    /// </summary>
    public static class NextActionFactory
    {
        public static INextAction GetNextAction(NextActionType actionType, int targetId, Task task, List<string> associatedAccounts = null)
        {
            switch (actionType)
            {
                case NextActionType.AssigningGroup:
                    return new AssigningGroupNextAction(targetId, task.CaseId);

                case NextActionType.SendingEmail:
                    return new SendingEmailNextAction(targetId);

                case NextActionType.CapturingData:
                    return new SavingCapturedData(targetId);

                case NextActionType.PostMessage:
                    return new PostMessageNextAction(targetId, task);

                case NextActionType.SyncDataWithFPS:
                    return new SyncDataNextAction(targetId, task.CaseId, associatedAccounts);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}