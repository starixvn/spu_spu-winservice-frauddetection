namespace FD.Lib.BusinessManager.UnitTest.TaskExecutors
{
    using Core;
    using Criteria.Task;
    using Lib.TaskExecutors;
    using NUnit.Framework;
    using Service.Implement;

    [TestFixture]
    public class ScanMatchTaskExecutorIntegrationTest : IntegrationTest
    {
        [Test]
        public void Run_Task_Scan_Match()
        {
            var taskService = new TaskService();

            var task = taskService.GetTaskForExecuting(new TaskGetForExecutingCriteria
            {
                TaskId = 1
            });
           
            // --- Arrange ---
            var sut = TaskExecutorFactory.GetTaskExecutor(task);

            // --- Action ---
            sut.Execute();

            // --- Assert ---
        }
    }
}