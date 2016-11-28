namespace FD.WinService.FraudDetection
{
    using System.Configuration;
    using System.ServiceProcess;
    using Core.QueueManagement;
    using Fanex.Data.Configuration;
    using Fanex.Logging;
    using Fanex.MailDelivery.Client.Handlers;
    using Lib.TaskLoaders;
    using Fanex.MailDelivery.Client;

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

            Logger.Log.Info("SPU FS Service Stopped");

            ReceivingMessageHandler.ClearTasks();

            MailClientHandler.Stop();
        }

        protected override void OnShutdown()
        {
            SendMailInform(false);

            Logger.Log.Info("SPU FS Service Stopped");

            ReceivingMessageHandler.ClearTasks();

            MailClientHandler.Stop();
        }

        public void RunOnStart()
        {
            Logger.Log.Info("SPU FS Service Started");

            MailClientHandler.Start();

            var queue = new MessageQueueReceiver();
            queue.Clear();

            TaskLoader.LoadTasks();

            ReceivingMessageHandler.TriggerReceivingMessage();
            SendMailInform(true);

            Logger.Log.Info("Loading tasks has done.");
        }

        private void SendMailInform(bool serviceStarting)
        {
            var subject = "SPU FS Service Started";
            if (!serviceStarting)
            {
                subject = "SPU FS Service Stopped";
            }

            var newMail = new Fanex.MailDelivery.Core.Models.Mail
            {
                Subject = subject,
                Title = subject,
                Body = subject,
                MailingListId = int.Parse(ConfigurationManager.AppSettings["MailingListId"])
            };

            MailClientHandler.SendFunctionMail(newMail);
        }

        private void InitDependencies()
        {
            InitializeComponent();
        }
    }
}