// --------------------------------------------------------------------------
// <copyright file="TaskExecutorFactory.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.TaskExecutors
{
    using System;
    using System.Globalization;
    using Lib.BusinessManager.TaskExecutors;
    using Lib.Core.Utilities;
    using Lib.DTO;
    using Lib.QueryGenerator;
    using Lib.TaskExecutors;

    public class TaskExecutorFactory
    {
        public static TaskExecutor GetTaskExecutor(Task task)
        {
            var eventType = Utility.ParseEnum<ActivityType>(task.EventId.ToString(CultureInfo.InvariantCulture));

            switch (eventType)
            {
                case ActivityType.ScanMathFindTicket:
                    return new ScanMatchTaskExecutor(task);

                case ActivityType.CountSuspiciousTimes:
                    return new CountingSuspicousTimesTaskExecutor(task);

                case ActivityType.ScanMatchFindBettingGroups:
                    return new FindBettingGroupsTaskExecutor(task);

                case ActivityType.ScanMatchCaptureData:
                    return new CaptureDataTaskExecutor(task);

                case ActivityType.ScanMatchCompareData:
                    return new CompareDataTaskExecutor(task);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}