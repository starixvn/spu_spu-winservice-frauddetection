// --------------------------------------------------------------------------
// <copyright file="IReceivingMessageHandler.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.QueueManagement
{
    using System.Messaging;

    /// <summary>
    /// Receiving Message Handler.
    /// </summary>
    public interface IReceivingMessageHandler
    {
        void HandleReceivedMessage(object sender, ReceiveCompletedEventArgs e);

        void TriggerReceivingMessage();

        void ClearTasks();
    }
}