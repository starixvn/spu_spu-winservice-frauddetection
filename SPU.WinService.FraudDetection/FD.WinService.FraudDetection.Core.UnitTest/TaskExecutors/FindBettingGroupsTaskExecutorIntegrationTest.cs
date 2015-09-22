namespace FD.WinService.FraudDetection.Core.UnitTest.TaskExecutors
{
    using Lib.Core;
    using Lib.Criteria.Task;
    using Lib.Service.Implement;
    using NUnit.Framework;
    using WinService.FraudDetection.Core.TaskExecutors;

    [TestFixture]
    public class FindBettingGroupsTaskExecutorIntegrationTest : IntegrationTest
    {
        [Test]
        public void Run_Task_Scan_Match()
        {
            var taskService = new TaskService();

            var task = taskService.GetTaskForExecuting(new TaskGetForExecutingCriteria
            {
                TaskId = 11
            });

            // --- Arrange ---
            var sut = TaskExecutorFactory.GetTaskExecutor(task);

            // --- Action ---
            sut.Execute();

            // --- Assert ---
        }
    }
}