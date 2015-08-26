// --------------------------------------------------------------------------
// <copyright file="EventType.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.Lib.TaskExecutors
{
    public enum EventType
    {
        /// <summary>
        /// No Event type.
        /// </summary>
        None = 0,

        /// <summary>
        /// Scan Match And Find Ticket.
        /// </summary>
        ScanMatchAndFindTicket = 1,

        /// <summary>
        /// Count Suspicious Times By Customer.
        /// </summary>
        CountSuspiciousTimesByCustomer = 2
    }
}