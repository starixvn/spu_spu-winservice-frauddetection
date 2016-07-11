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

    /// <summary>
    ///     ActionFactory Class.
    /// </summary>
    public static class NextActionFactory
    {
        public static INextAction GetNextAction(NextActionType actionType, int targetId, Task task)
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

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}