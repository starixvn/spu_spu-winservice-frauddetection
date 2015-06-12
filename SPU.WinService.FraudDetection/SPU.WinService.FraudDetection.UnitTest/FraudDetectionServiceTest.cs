using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using SPU.Lib.FraudDetection;

namespace SPU.WinService.FraudDetection.UnitTest
{
    [TestFixture]
    public class FraudDetectionServiceTest
    {
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
        public void StartServiceAsNormal()
        {
            var sut = new FraudDetectionService();

            // ---- Act ---
            sut.RunOnStart();

            // --- Assert  ---

            //Thread.Sleep(TimeSpan.FromSeconds(5));

            //Assert.AreEqual(4, sut.ReceivingMessageHandler.ReceiveMessageCount);
        }
    }
}