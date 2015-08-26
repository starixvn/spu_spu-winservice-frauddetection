namespace FD.Lib.BusinessManager.UnitTest.TaskLoader
{
    using System;
    using System.Collections.Generic;
    using DTO;
    using Fanex.Data.Configuration;
    using Fanex.Data.Repository;
    using Moq;
    using NUnit.Framework;
    using QueueManagement;
    using QueueManagement.SendingTasks;
    using Service.Interface;
    using TaskLoaders;
    using TaskLoader = TaskLoaders.TaskLoader;

    public class TaskLoaderTest
    {
        private Mock<IMessageQueueSender> _mockQueue;
        private Mock<ITaskService> _mockTaskService;
        private ITaskLoader _sut;

        [SetUp]
        public void Setup()
        {
            DbSettingProviderManager.Current.Start();
            _mockTaskService = new Mock<ITaskService>();
            _mockQueue = new Mock<IMessageQueueSender>();
            _sut = new TaskLoader(_mockTaskService.Object, _mockQueue.Object);
        }

        public Task GetSampleTask()
        {
            var sampleTask = new Task
            {
                EventId = 2,
                Interval = 60,
                IntervalType = "Second",
                IsBackgroundProcess = true,
                TaskPreconditions =
                    @"select top 100 tch.matchid from bodb02.dbo.match tch left join dbo.ScannedMatches hes on tch.matchid = hes.matchid  where tch.matchid in (select tch.matchid from bodb02.dbo.match tch with(nolock) where tch.matchid not in (select hes.MatchId from dbo.ScannedMatches hes where hes.CaseId = $param1$)) and (tch.eventdate < '6/4/2015' AND tch.eventstatus = 'completed' )",
                TaskRules =
                    "select ans.custid,ans.matchid,ans.transid from bodb02.dbo.match tch inner join bodb02.dbo.bettrans ans on tch.matchid = ans.matchid  where tch.matchid in ($param1$) and (ans.stake >= 300 AND ans.statusid = 112)",
                CaseId = 4,
                TaskName = "Scan Match - Find Ticket",
                TaskId = 80
            };

            return sampleTask;
        }

        #region Tests For Calling LoadTasks

        [Test]
        public void LoadAllTasks_Should_Be_Called()
        {
            //--- Act ---
            _sut.LoadTasks();

            //--- Assert ---
            _mockTaskService.Verify(x => x.LoadAllTasks(It.IsAny<ICriteria>()), Times.Exactly(1));
        }

        [Test]
        public void Send_Should_Called_Once_When_One_Active_Task()
        {
            //--- Arrange ---
            var sampleTask = GetSampleTask();
            sampleTask.IsActive = true;
            var sampleTasks = new List<Task> { sampleTask };
            _mockTaskService.Setup(x => x.LoadAllTasks(It.IsAny<ICriteria>())).Returns(sampleTasks);

            //--- Act ---
            _sut.LoadTasks();

            //--- Assert ---
            _mockQueue.Verify(
                x => x.Send(
                    It.IsAny<object>(),
                    It.Is<string>(t => t.Equals(SendingType.AddingTask.ToString(), StringComparison.OrdinalIgnoreCase))),
                    Times.Exactly(1));
        }

        [Test]
        public void Send_Should_Not_Called_Once_When_One_Inactive_Task()
        {
            //--- Arrange ---
            var sampleTask = GetSampleTask();
            sampleTask.IsActive = false;
            var sampleTasks = new List<Task> { sampleTask };

            _mockTaskService.Setup(x => x.LoadAllTasks(It.IsAny<ICriteria>())).Returns(sampleTasks);

            //--- Act ---
            _sut.LoadTasks();

            //--- Assert ---
            _mockQueue.Verify(x => x.Send(It.IsAny<object>()), Times.Exactly(0));
        }

        [Test]
        public void Send_Should_Called_Twice_When_Two_Active_Tasks()
        {
            //--- Arrange ---
            var sampleTask1 = GetSampleTask();
            sampleTask1.IsActive = true;

            var sampleTask2 = GetSampleTask();
            sampleTask2.IsActive = true;

            var sampleTasks = new List<Task> { sampleTask1, sampleTask2 };
            _mockTaskService.Setup(x => x.LoadAllTasks(It.IsAny<ICriteria>())).Returns(sampleTasks);

            //--- Act ---
            _sut.LoadTasks();

            //--- Assert ---
            _mockQueue.Verify(
                x => x.Send(
                    It.IsAny<object>(),
                    It.Is<string>(t => t.Equals(SendingType.AddingTask.ToString(), StringComparison.OrdinalIgnoreCase))),
                    Times.Exactly(2));
        }

        public void Real_Send_Should_Called_Twice_When_Two_Active_Tasks()
        {
            //--- Arrange ---
            var sut = new TaskLoader();

            var queue = new MessageQueueSender();
            queue.Clear();

            //--- Act ---
            sut.LoadTasks();

            // while (true) {}
        }

        #endregion Tests For Calling LoadTasks
    }
}