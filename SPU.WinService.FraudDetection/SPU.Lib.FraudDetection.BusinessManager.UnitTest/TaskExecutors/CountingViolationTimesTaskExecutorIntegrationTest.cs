namespace FD.Lib.BusinessManager.UnitTest.TaskExecutors
{
    using Core;
    using Criteria.Task;
    using Lib.TaskExecutors;
    using NUnit.Framework;
    using Service.Implement;

    [TestFixture]
    public class CountingViolationTimesTaskExecutorIntegrationTest : IntegrationTest
    {
        [Test]
        public void Run_Task_Count_Suspicious_Times()
        {
            var taskService = new TaskService();
            var task = taskService.GetTaskForExecuting(new TaskGetForExecutingCriteria
            {
                TaskId = 2
            });

            // --- Arrange ---
            var sut = TaskExecutorFactory.GetTaskExecutor(task);

            // --- Action ---
            sut.Execute();

            // --- Assert ---
        }
    }
}