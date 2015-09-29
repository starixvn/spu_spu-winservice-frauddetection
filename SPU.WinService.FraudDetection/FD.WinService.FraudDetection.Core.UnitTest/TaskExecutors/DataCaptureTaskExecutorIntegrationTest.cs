namespace FD.WinService.FraudDetection.Core.UnitTest.TaskExecutors
{
    using Core.TaskExecutors;
    using Lib.Core;
    using Lib.Criteria.Task;
    using Lib.Service.Implement;
    using NUnit.Framework;

    [TestFixture]
    public class DataCaptureTaskExecutorIntegrationTest : IntegrationTest
    {
        [Test]
        public void Run_Task_Data_Capture()
        {
            var taskService = new TaskService();

            var task = taskService.GetTaskForExecuting(new TaskGetForExecutingCriteria
            {
                TaskId = 19
            });

            // --- Arrange ---
            var sut = TaskExecutorFactory.GetTaskExecutor(task);

            // --- Action ---
            sut.Execute();

            // --- Assert ---
        }
    }
}