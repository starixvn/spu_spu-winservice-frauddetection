namespace FD.WinService.FraudDetection
{
    using System.Diagnostics;
    using System.ServiceProcess;
    using Core.QueueManagement;
    using Fanex.Data.Configuration;
    using Fanex.MailDelivery.Client.Handlers;
    using Lib.QueueManagement;
    using Lib.TaskLoaders;

    public partial class FraudDetectionService : ServiceBase
    {
        public readonly ITaskLoader TaskLoader;
        public readonly IReceivingMessageHandler ReceivingMessageHandler;

        public FraudDetectionService()
        {
            InitDependencies();

            TaskLoader = new TaskLoader();
            ReceivingMessageHandler = new ReceivingMessageHandler();

            DbSettingProviderManager.Current.Start();
        }

        private void InitDependencies()
        {
            InitializeComponent();

            eventLog1 = new EventLog();

            if (!EventLog.SourceExists("MySource"))
            {
                EventLog.CreateEventSource("MySource", "MyNewLog");
            }

            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
            eventLog1.WriteEntry("Installing Fraud Detection Windows Service");
        }

        public FraudDetectionService(ITaskLoader taskLoader, IReceivingMessageHandler receivingMessageHandler)
        {
            InitDependencies();

            TaskLoader = taskLoader;
            ReceivingMessageHandler = receivingMessageHandler;
        }

        protected override void OnStart(string[] args)
        {
            RunOnStart();
        }

        protected override void OnStop()
        {
            ReceivingMessageHandler.ClearTasks();
            MailClientHandler.Stop();
        }

        protected override void OnShutdown()
        {
            ReceivingMessageHandler.ClearTasks();
        }

        public void RunOnStart()
        {
            MailClientHandler.Start();

            var queue = new MessageQueueReceiver();
            queue.Clear();
            ReceivingMessageHandler.TriggerReceivingMessage();
            TaskLoader.LoadTasks();
        }
    }
}