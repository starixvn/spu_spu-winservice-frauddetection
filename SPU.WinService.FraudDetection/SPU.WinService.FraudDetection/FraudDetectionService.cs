namespace FD.WinService.FraudDetection
{
    using System.Configuration;
    using System.ServiceProcess;
    using Core.QueueManagement;
    using Fanex.Data.Configuration;
    using Fanex.Logging;
    using Fanex.MailDelivery.Client.Handlers;
    using Lib.TaskLoaders;

    public partial class FraudDetectionService: ServiceBase
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
            SendMailInform(false);

            Logger.Log.Error("SPU FS Service Stopped");

            ReceivingMessageHandler.ClearTasks();

            MailClientHandler.Stop();
        }

        protected override void OnShutdown()
        {
            SendMailInform(false);

            //// Logger.Log.Error("Stop Fraud-Detection-Service service", "Fraud-Detection-Service-UAT");
            Logger.Log.Error("SPU FS Service Stopped");

            ReceivingMessageHandler.ClearTasks();

            MailClientHandler.Stop();
        }

        public void RunOnStart()
        {
            //// Logger.Log.Error("Starting Fraud-Detection-Service service", "Fraud-Detection-Service-UAT");
            Logger.Log.Error("SPU FS Service Started");

            MailClientHandler.Start();

            var queue = new MessageQueueReceiver();
            queue.Clear();

            TaskLoader.LoadTasks();

            ReceivingMessageHandler.TriggerReceivingMessage();
            SendMailInform(true);

            Logger.Log.Error("Loading tasks has done.", "Fraud-Detection-Service-UAT");
        }

        private void SendMailInform(bool serviceStarting)
        {
            var subject = "SPU FS Service Started";
            if (!serviceStarting)
            {
                subject = "SPU FS Service Stopped";
            }
            var newMail = new Fanex.MailDelivery.Client.Models.Mail
            {
                Subject = subject,
                Title = subject,
                Body = subject,
                MailingListId = int.Parse(ConfigurationManager.AppSettings["MailingListId"])
            };

            MailClientHandler.SendMail(newMail);
        }
    }
}