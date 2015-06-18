using SPU.Lib.FraudDetection;

namespace SPU.WinService.FraudDetection.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var queue = new SpuMessageQueue();
            queue.Clear();

            var sut = new FraudDetectionService();

            System.Console.WriteLine("Start to run window service manually.");

            sut.RunOnStart();

            System.Console.WriteLine("Window service is running now.");
            while (true) ;
        }
    }
}