namespace FD.Lib.BusinessManager.UnitTest
{
    using System;
    using BusinessManager.TaskExecutors;
    using Moq;
    using NUnit.Framework;
    using QueueManagement;
    using QueueManagement.SendingTasks;

    [TestFixture]
    public class AddingTaskTest
    {
        private Mock<IMessageQueueSender> _mockQueue;
        private ISendingMessage _sut;

        [SetUp]
        public void Setup()
        {
            _mockQueue = new Mock<IMessageQueueSender>();
            _sut = new AddingTask(_mockQueue.Object);
        }

        [Test]
        public void When_Adding_Not_Null_Task_Then_Send_Should_Called_Once_()
        {
            var result = _sut.Send(1);

            //--- Assert ---
            _mockQueue.Verify(
                x => x.Send(
                    It.IsAny<object>(),
                    It.Is<string>(t => t.Equals(_sut.Type.ToString(), StringComparison.OrdinalIgnoreCase))),
                    Times.Exactly(1));
                Assert.IsTrue(result);
        }

        [Test]
        public void Send_Should_Not_Be_Called_Once_When_Adding_Null_Task()
        {
            //--- Act ---
            var result = _sut.Send(0);

            //--- Assert ---
            _mockQueue.Verify(
                x => x.Send(
                    It.IsAny<TaskExecutor>(),
                    It.Is<string>(t => t.Equals(_sut.Type.ToString(), StringComparison.OrdinalIgnoreCase))),
                    Times.Exactly(0));
                Assert.IsFalse(result);
        }
    }
}