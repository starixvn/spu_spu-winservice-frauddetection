namespace FD.WinService.FraudDetection.Core.QueueManagement
{
    using System;
    using System.CodeDom;
    using System.Messaging;
    using Lib.QueueManagement;

    public interface IMessageQueueReceiver
    {
        void AttachReceiveCompletedEventHandler(ReceiveCompletedEventHandler receiveCompleted);

        void DeattachReceiveCompletedEventHandler(ReceiveCompletedEventHandler receiveCompleted);

        void Stop();

        void Clear();

        void BeginReceive();

        Message EndReceive(IAsyncResult asyncResult);
    }

    public class MessageQueueReceiver : FDMessageQueue, IMessageQueueReceiver
    {
        public void AttachReceiveCompletedEventHandler(ReceiveCompletedEventHandler receiveCompleted)
        {
            _messageQueue.ReceiveCompleted += receiveCompleted;
        }

        public void DeattachReceiveCompletedEventHandler(ReceiveCompletedEventHandler receiveCompleted)
        {
            _messageQueue.ReceiveCompleted -= receiveCompleted;
        }

        public void Stop()
        {
            _messageQueue.Purge();
            _messageQueue.Close();
        }

        public void Clear()
        {
            _messageQueue.Purge();
        }

        public void BeginReceive()
        {
            _messageQueue.BeginReceive();
        }

        public Message EndReceive(IAsyncResult asyncResult)
        {
            return _messageQueue.EndReceive(asyncResult);
        }
    }
}