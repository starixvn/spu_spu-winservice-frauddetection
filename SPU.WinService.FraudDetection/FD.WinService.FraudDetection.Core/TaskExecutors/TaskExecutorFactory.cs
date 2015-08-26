// --------------------------------------------------------------------------
// <copyright file="TaskExecutorFactory.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.Lib.TaskExecutors
{
    using System;
    using BusinessManager.TaskExecutors;
    using Core.Utilities;
    using DTO;

    public class TaskExecutorFactory
    {
        public static TaskExecutor GetTaskExecutor(Task task)
        {
            var eventType = Utility.ParseEnum<EventType>(task.EventId.ToString());

            switch (eventType)
            {
                case EventType.ScanMatchAndFindTicket:
                    return new ScanMatchTaskExecutor(task);

                case EventType.CountSuspiciousTimesByCustomer:
                    return new CountingSuspicousTimesTaskExecutor(task);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}