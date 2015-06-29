using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Fanex.Data.Configuration;

namespace SPU.WinService.FraudDetection
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            var servicesToRun = new ServiceBase[]
            {
                new FraudDetectionService(),
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}