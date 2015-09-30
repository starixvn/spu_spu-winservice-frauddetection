namespace FD.WinService.FraudDetection
{
    using System.ServiceProcess;
    using Core.QueueManagement;
    using Fanex.Data.Configuration;
    using Fanex.Logging;
    using Fanex.MailDelivery.Client.Handlers;
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
            Logger.Log.Error("Starting Fraud-Detection-Service service", "Fraud-Detection-Service-UAT");

            MailClientHandler.Start();

            var queue = new MessageQueueReceiver();
            queue.Clear();
            ReceivingMessageHandler.TriggerReceivingMessage();
            TaskLoader.LoadTasks();

            Logger.Log.Error("Loading tasks has done.", "Fraud-Detection-Service-UAT");
        }
    }
}