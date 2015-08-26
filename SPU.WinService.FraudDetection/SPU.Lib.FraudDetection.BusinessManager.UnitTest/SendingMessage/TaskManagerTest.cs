namespace FD.Lib.BusinessManager.UnitTest
{
    using BusinessManager.TaskExecutors;
    using DTO;
    using Fanex.Data.Repository;
    using Lib.TaskExecutors;
    using Moq;
    using NUnit.Framework;
    using Service.Interface;

    [TestFixture]
    public class TaskManagerTest
    {
        private TaskManager _sut;
        private Mock<ITaskService> _mockTaskService;

        [SetUp]
        public void SetUp()
        {
            _mockTaskService = new Mock<ITaskService>();
            _sut = new TaskManager(_mockTaskService.Object);
        }

        [Test]
        public void When_Add_Task_Then_One_TaskTimer_Is_Created()
        {
            // --- Arrange ---
            var task = GetSampleTask();
            _mockTaskService
                .Setup(x => x.GetTaskForExecuting(It.IsAny<ICriteria>()))
                .Returns(task);

            // --- Act ---
            _sut.AddTask(1);

            // --- Assert ---
            Assert.AreEqual(1, _sut.TaskExecutors.Count);
        }

        [Test]
        public void When_Add_Two_Tasks_But_The_Same_Then_Only_One_TaskTimer_Is_Created()
        {
            // --- Arrange ---
            var task = GetSampleTask();

            _mockTaskService
                .Setup(x => x.GetTaskForExecuting(It.IsAny<ICriteria>()))
                .Returns(task);

            // --- Act ---
            _sut.AddTask(1);

            _sut.AddTask(1);

            // --- Assert ---
            Assert.AreEqual(1, _sut.TaskExecutors.Count);
        }

        [Test]
        public void When_Removed_Existed_Task_Then_TaskTimer_Count_Reduce_One()
        {
            // --- Arrange ---
            var task = GetSampleTask();
            
            _mockTaskService
                .Setup(x => x.GetTaskForExecuting(It.IsAny<ICriteria>()))
                .Returns(task);

            _sut.AddTask(80);

            // --- Act ---
            _sut.RemoveTask(80);

            // --- Assert ---
            var actualCount = _sut.TaskExecutors.Count;

            Assert.AreEqual(0, actualCount);
        }

        [Test]
        public void When_Removed_Non_Existed_Task_Then_TaskTimer_Count_Does_Not_Reduce()
        {
            // --- Arrange ---
            var task = GetSampleTask();

            _mockTaskService
                .Setup(x => x.GetTaskForExecuting(It.IsAny<ICriteria>()))
                .Returns(task);

            _sut.AddTask(1);

            var nonExistTask = new Task
            {
                CaseId = 5,
                TaskId = 6
            };

            _mockTaskService
                .Setup(x => x.GetTaskInfoByTaskId(It.IsAny<ICriteria>()))
                .Returns(nonExistTask);

            // --- Act ---
            _sut.RemoveTask(6);

            // --- Assert ---
            var actualCount = _sut.TaskExecutors.Count;
            Assert.AreEqual(1, actualCount);
        }

        [Test]
        public void When_Updated_Existed_Task_Then_TaskTimer_Count_Does_Not_Increase()
        {
            // --- Arrange ---
            var task = GetSampleTask();

            _mockTaskService
                .Setup(x => x.GetTaskForExecuting(It.IsAny<ICriteria>()))
                .Returns(task);

            _sut.AddTask(1);

            // --- Act ---
            const string taskName = "Task Demo";
            task.TaskName = taskName;

            _mockTaskService
                .Setup(x => x.GetTaskInfoByTaskId(It.IsAny<ICriteria>()))
                .Returns(task);

            _sut.UpdateTask(1);

            // --- Assert ---
            var actualCount = _sut.TaskExecutors.Count;
            Assert.AreEqual(1, actualCount);
            Assert.AreEqual(taskName, _sut.TaskExecutors[0].Task.TaskName);
        }

        [Test]
        public void When_Updated_Non_Existed_Task_Then_TaskTimer_Count_Increase_One()
        {
            // --- Arrange ---
            var task = GetSampleTask();

            _mockTaskService
                .Setup(x => x.GetTaskForExecuting(It.IsAny<ICriteria>()))
                .Returns(task);

            _sut.AddTask(1);

            // --- Act ---
            var nonExistTask = new Task
            {
                EventId = 1,
                TaskName = "New Task",
                CaseId = 9,
                TaskId = 99,
                Interval = 90,
                IntervalType = "Second",
                IsActive = true
            };

            _mockTaskService
               .Setup(x => x.GetTaskForExecuting(It.IsAny<ICriteria>()))
               .Returns(nonExistTask);

            _sut.UpdateTask(99);

            // --- Assert ---
            var actualCount = _sut.TaskExecutors.Count;
            Assert.AreEqual(2, actualCount);
            Assert.AreEqual(nonExistTask.TaskId, _sut.TaskExecutors[1].TaskId);
            Assert.AreEqual(nonExistTask.TaskName, _sut.TaskExecutors[1].Task.TaskName);
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
                    @"select top 100 tch.matchid from bodb02.dbo.match tch left join dbo.ScannedMatches hes on tch.matchid = hes.matchid  where tch.matchid in (select tch.matchid from bodb02.dbo.match tch with(nolock) where tch.matchid not in (select hes.MatchId from dbo.ScannedMatches hes where hes.CaseId = $param1$)) and (tch.eventdate < '6/4/2015' AND tch.eventstatus = 'completed' )",
                TaskRules =
                    "select ans.custid,ans.matchid,ans.transid from bodb02.dbo.match tch inner join bodb02.dbo.bettrans ans on tch.matchid = ans.matchid  where tch.matchid in ($param1$) and (ans.stake >= 300 AND ans.statusid = 112)",
                CaseId = 4,
                TaskName = "Scan Match - Find Ticket",
                TaskId = 80,
                IsActive = true
            };

            return sampleTask;
        }
    }
}