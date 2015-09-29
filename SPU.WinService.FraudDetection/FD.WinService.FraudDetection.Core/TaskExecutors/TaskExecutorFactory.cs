// --------------------------------------------------------------------------
// <copyright file="TaskExecutorFactory.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

using System.Globalization;

namespace FD.WinService.FraudDetection.Core.TaskExecutors
{
    using System;
    using Lib.BusinessManager.TaskExecutors;
    using Lib.Core.Utilities;
    using Lib.DTO;
    using Lib.TaskExecutors;

    public class TaskExecutorFactory
    {
        public static TaskExecutor GetTaskExecutor(Task task)
        {
            var eventType = Utility.ParseEnum<EventType>(task.EventId.ToString(CultureInfo.InvariantCulture));

            switch (eventType)
            {
                case EventType.ScanMatchAndFindTicket:
                    return new ScanMatchTaskExecutor(task);

                case EventType.CountSuspiciousTimesByCustomer:
                    return new CountingSuspicousTimesTaskExecutor(task);

                case EventType.ScanMatchAndFindBettingGroups:
                    return new FindBettingGroupsTaskExecutor(task);

                case EventType.ScanMatchAndCaptureData:
                    return new CaptureDataTaskExecutor(task);

                case EventType.MatchChecking:
                    return new MatchCheckingTaskExecutor(task);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}