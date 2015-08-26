// --------------------------------------------------------------------------
// <copyright file="DeactivateCaseTest.cs" company="Nexcel Solutions Vietnam">
// Copyright (c) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.06.12">Created</change>
// </history>

namespace FD.Lib.BusinessManager.UnitTest
{
    using System;
    using BusinessManager.TaskExecutors;
    using Lib.TaskExecutors;
    using Moq;
    using NUnit.Framework;
    using QueueManagement;
    using QueueManagement.SendingTasks;
    using QueueManagement.SendingWorklows;

    [TestFixture]
    public class DeactivateCaseTest
    {
        private Mock<IMessageQueueSender> _mockQueue;
        private ISendingMessage _sut;

        [SetUp]
        public void Setup()
        {
            _mockQueue = new Mock<IMessageQueueSender>();
            _sut = new DeactivatingCase();
        }

        [Test]
        [Ignore("Ignore a fixture")]
        public void When_Removing_Valid_CaseId_Then_Send_Should_Called_Once_()
        {
            //--- Act ---
            const int worflowId = 1;
            var result = _sut.Send(worflowId);

            //--- Assert ---
            _mockQueue.Verify(
                x => x.Send(
                    It.IsAny<int>(),
                    It.Is<string>(t => t.Equals(_sut.Type.ToString(), StringComparison.OrdinalIgnoreCase))),
                    Times.Exactly(1));
                Assert.IsTrue(result);
        }

        [Test]
        [Ignore("Ignore a fixture")]
        public void Send_Should_Not_Be_Called_Once_When_Removing_Null_Task()
        {
            //--- Act ---
            var result = _sut.Send(null);

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