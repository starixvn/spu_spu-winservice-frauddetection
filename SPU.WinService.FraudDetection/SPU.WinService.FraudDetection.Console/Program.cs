namespace FD.WinService.FraudDetection.Console
{
    using System;
    using Core.QueueManagement;
    using Lib.QueueManagement;
    using SPU.WinService.FraudDetection;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var queue = new MessageQueueReceiver();
            queue.Clear();

            var sut = new FraudDetectionService();

            Console.WriteLine("Start to run window service manually.");

            sut.RunOnStart();

            Console.WriteLine("Window service is running now.");
            while (true) ;
        }
    }
}