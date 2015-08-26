// --------------------------------------------------------------------------
// <copyright file="ReceivingMessageHandler.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.QueueManagement
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Messaging;
    using System.Threading.Tasks;
    using Lib.BusinessManager.TaskExecutors;
    using Lib.QueueManagement.SendingTasks;
    using Lib.TaskExecutors;

    /// <summary>
    ///     Receiving Message Handler.
    /// </summary>
    public class ReceivingMessageHandler : IReceivingMessageHandler
    {
        private readonly IMessageQueueReceiver _queue;
        private readonly ITaskManager _taskManager;
        private Task _currentThread;

        public ReceivingMessageHandler()
        {
            _taskManager = new TaskManager();
            _queue = new MessageQueueReceiver();
            _queue.AttachReceiveCompletedEventHandler(HandleReceivedMessage);
        }

        public ReceivingMessageHandler(ITaskManager taskManager, IMessageQueueReceiver queue)
        {
            _taskManager = taskManager;
            _queue = queue;
            _queue.AttachReceiveCompletedEventHandler(HandleReceivedMessage);
        }

        [SuppressMessage(
              "StyleCop.CSharp.Nexcel.NexcelCustomRules",
              "SP2101:MethodMustNotContainMoreLinesThan",
              Justification = "Reviewed. Suppression is OK here.")]
        public void HandleReceivedMessage(object sender, ReceiveCompletedEventArgs e)
        {
            using (var msg = _queue.EndReceive(e.AsyncResult))
            {
                var message = msg.Body;
                var id = (int)message;
                var label = msg.Label;

                if (string.IsNullOrEmpty(label))
                {
                    return;
                }

                _currentThread = _currentThread == null
                    ? Task.Factory.StartNew(() => HandleMessage(label, id))
                    : _currentThread.ContinueWith((antecedent) => HandleMessage(label, id));
            }

            _queue.BeginReceive();
        }

        private void HandleMessage(string label, int id)
        {
            var type = ConvertLabelIntoSendingType(label);

            switch (type)
            {
                case SendingType.AddingTask:
#if DEBUG
                    Console.WriteLine("AddingTask: {0}", id);
#endif

                    _taskManager.AddTask(id);
                    break;

                case SendingType.UpdatingTask:
#if DEBUG
                    Console.WriteLine("UpdatingTask: {0}", id);
#endif

                    _taskManager.UpdateTask(id);
                    break;

                case SendingType.RemovingTask:
#if DEBUG
                    Console.WriteLine("RemovingTask: {0}{0}", id);
#endif

                    _taskManager.RemoveTask(id);
                    break;

                case SendingType.DeactivatingCase:
#if DEBUG
                    Console.WriteLine("DeactivateCase: {0}", id);
#endif
                    _taskManager.RemoveTasksInCase(id);
                    break;

                case SendingType.ActivatingCase:
#if DEBUG
                    Console.WriteLine("ActivateCase: {0}", id);
#endif
                    _taskManager.AddTasksInCase(id);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void TriggerReceivingMessage()
        {
            _queue.BeginReceive();
        }

        public void ClearTasks()
        {
            _taskManager.ClearTasks();
            _queue.DeattachReceiveCompletedEventHandler(HandleReceivedMessage);
        }

        private SendingType ConvertLabelIntoSendingType(string label)
        {
            SendingType type;
            var result = Enum.TryParse(label, true, out type);

            return result ? type : SendingType.None;
        }
    }
}