namespace FD.WinService.FraudDetection.Console
{
    using System;
    using Core.QueueManagement;

    internal class Program
    {
        private static void Main()
        {
            var sut = new FraudDetectionService();

            Console.WriteLine("Start to run window service manually.");

            sut.RunOnStart();

            Console.WriteLine("Window service is running now.");
            while (true)
            {
            }
        }
    }
}