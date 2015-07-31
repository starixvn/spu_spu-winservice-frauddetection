namespace FD.WinService.FraudDetection.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fanex.Data.Repository;
    using FD.Lib.QueueManagement;
    using FD.Lib.QueueManagement.SendingTasks;
    using FD.Lib.Service.Interface;
    using FD.Lib.TaskExecutors;
    using FD.Lib.TaskLoaders;
    using FD.WinService.FraudDetection;
    using Lib.DTO;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FraudDetectionServiceTest
    {
        private FDMessageQueue _mockSpuQueue;

        private ReceivingMessageHandler _mockMessageHandler;

        private Mock<ITaskService> _mockTaskService;

        private Mock<ITaskManager> _mockTaskManager;

        private TaskLoader _mockTaskLoader;

        private FraudDetectionService _sut;

        [SetUp]
        public void SetUp()
        {
            _mockSpuQueue = new FDMessageQueue();
            _mockTaskService = new Mock<ITaskService>();
            _mockTaskManager = new Mock<ITaskManager>();
            _mockTaskLoader = new TaskLoader(_mockTaskService.Object, _mockSpuQueue);
            _mockMessageHandler = new ReceivingMessageHandler(_mockTaskManager.Object, _mockSpuQueue);
            _sut = new FraudDetectionService(_mockTaskLoader, _mockMessageHandler);
            _mockSpuQueue.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            _mockSpuQueue.Stop();
        }

        [Test]
        public void When_Call_OnStart_Then_Load_Tasks_Should_Be_Called()
        {
            // ---- Arrange ---
            var mockTaskLoader = new Mock<ITaskLoader>();
            var mockReceivingMessageHandler = new Mock<IReceivingMessageHandler>();
            var sut = new FraudDetectionService(mockTaskLoader.Object, mockReceivingMessageHandler.Object);

            // ---- Act ---
            sut.RunOnStart();

            // ---- Assert ---
            mockReceivingMessageHandler.Verify(x => x.TriggerReceivingMessage(), Times.Exactly(1));
            mockTaskLoader.Verify(x => x.LoadTasks(), Times.Exactly(1));
        }

        [Test]
        public void When_Load_No_Task_Then_AddTask_Should_Not_Be_Called()
        {
            _mockTaskService.Setup(x => x.LoadAllTasks(It.IsAny<ICriteria>())).Returns(new List<Task>());

            // ---- Act ---
            _sut.RunOnStart();

            // --- Assert  ---
            _mockTaskManager.Verify(x => x.AddTask(It.IsAny<int>()), Times.Exactly(0));
        }

        [Test]
        public void When_Load_Two_Tasks_Then_AddTask_Should_Be_Called_Twice()
        {
            var sampleTask = GetSampleTask();
            sampleTask.IsActive = true;
            var sampleTasks = new List<Task> { sampleTask, sampleTask };

            _mockTaskService.Setup(x => x.LoadAllTasks(It.IsAny<ICriteria>())).Returns(sampleTasks);

            // ---- Act ---
            _sut.RunOnStart();

            // --- Assert  ---
            Thread.Sleep(TimeSpan.FromSeconds(2));

            _mockTaskManager.Verify(x => x.AddTask(It.IsAny<int>()), Times.Exactly(2));
        }

        [Test]
        public void When_Update_One_Task_Then_UpdateTask_Should_Be_Called_Once()
        {
            var sampleTask = GetSampleTask();
            sampleTask.IsActive = true;
            var sampleTasks = new List<Task> { sampleTask, sampleTask };
            var updatingTask = SendingMessageFactory.GetSender(SendingType.UpdatingTask);

            _mockTaskService.Setup(x => x.LoadAllTasks(It.IsAny<ICriteria>())).Returns(sampleTasks);

            // ---- Act ---
            _sut.RunOnStart();

            updatingTask.Send(sampleTask);

            // --- Assert  ---
            Thread.Sleep(TimeSpan.FromSeconds(2));

            _mockTaskManager.Verify(x => x.UpdateTask(It.IsAny<int>()), Times.Exactly(1));
        }

        [Test]
        public void When_Remove_One_Task_Then_RemoveTask_Should_Be_Called_Once()
        {
            var sampleTask = GetSampleTask();
            sampleTask.IsActive = true;
            var sampleTasks = new List<Task> { sampleTask, sampleTask };
            var removeingTask = SendingMessageFactory.GetSender(SendingType.RemovingTask);

            _mockTaskService.Setup(x => x.LoadAllTasks(It.IsAny<ICriteria>())).Returns(sampleTasks);

            // ---- Act ---
            _sut.RunOnStart();

            removeingTask.Send(sampleTask);

            // --- Assert  ---
            Thread.Sleep(TimeSpan.FromSeconds(3));

            _mockTaskManager.Verify(x => x.RemoveTask(It.IsAny<int>()), Times.Exactly(1));
        }

        private Task GetSampleTask()
        {
            var sampleTask = new Task
            {
                EventId = 2,
                Interval = 60,
                IntervalType = "Second",
                IsBackgroundProcess = true,
                TaskPreconditions =
                    @"select top 100 tch.matchid from bodb02.dbo.match tch left join dbo.ScannedMatches hes on tch.matchid = hes.matchid  where tch.matchid in (select tch.matchid from bodb02.dbo.match tch with(nolock) where tch.matchid not in (select hes.MatchId from dbo.ScannedMatches hes where hes.WorkflowId = $param1$)) and (tch.eventdate < '6/4/2015' AND tch.eventstatus = 'completed' )",
                TaskRules =
                    "select ans.custid,ans.matchid,ans.transid from bodb02.dbo.match tch inner join bodb02.dbo.bettrans ans on tch.matchid = ans.matchid  where tch.matchid in ($param1$) and (ans.stake >= 300 AND ans.statusid = 112)",
                CaseId = 4,
                TaskName = "Scan Match - Find Ticket",
                TaskId = 80
            };

            return sampleTask;
        }
    }
}