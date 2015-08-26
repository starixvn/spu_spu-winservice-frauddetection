namespace SPU.Lib.FraudDetection.BusinessManager.UnitTest.MailSenders
{
    using System.Collections.Generic;
    using Fanex.Data.Configuration;
    using FD.Lib.BusinessManager.ReportTemplateRenders;
    using FD.Lib.BusinessManager.UnitTest.Mock_Objects;
    using FD.Lib.DTO;
    using FD.Lib.MailMasterRenders;
    using FD.Lib.MailSenders;
    using FD.Lib.Model.ReportTemplate;
    using FD.Lib.ReportTemplateRenders;
    using FD.Lib.Service.Implement;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SpuMailSenderTest
    {
        [SetUp]
        public void SetUp()
        {
            DbSettingProviderManager.Current.Start();
        }

        [Test]
        public void When_Calling_Send_Mail_Then_RenderContent_Should_Be_Called()
        {
            // --- Arrange --
            const string partialContent = "Suspicious Statistic Table and Margin Table";
            var reporRenderAbstractFactory = new Mock<IReportRenderAbstractFactory>();
            var mockMailRender = new Mock<IReportTemplateRender>();
            var mockMailRenderAbstractFactory = new Mock<IMailMasterRenderAbstractFactory>();
            var mockMailMasterRender = new MailMasterRender(reporRenderAbstractFactory.Object);
            reporRenderAbstractFactory.Setup(x => x.GetReportTemplateRender()).Returns(mockMailRender.Object);
            mockMailRenderAbstractFactory.Setup(x => x.GetMailMasterRender()).Returns(mockMailMasterRender);
            mockMailRender.Setup(x => x.RenderContent()).Returns(partialContent);

            // --- Act --
            var sut = new FDMailSender(new FakeMailClient())
            {
                MailMasterRenderAbstractFactory = mockMailRenderAbstractFactory.Object
            };

            sut.SendMail(1);

            // --- Assert --
            mockMailRender.Verify(x => x.RenderContent(), Times.Exactly(1));
        }

        [Test]
        [Ignore("Ignore a fixture")]
        public void Test_Intergrating_Mail_Master_Into_System()
        {
            var model = GetSampleData();

            var renderFactory = new SuspiciousStatisticReporRenderAbstractFactory
            {
                SuspiciousStatisticViewModel = model
            };

            var mailMasterRenderAbstractFactory = new MailMasterRenderAbstractFactory
            {
                ReportRenderFactory = renderFactory
            };

            var sut = new FDMailSender(new FakeMailClient())
            {
                MailMasterRenderAbstractFactory = mailMasterRenderAbstractFactory
            };

            sut.SendMail(1);
        }

        private SuspiciousStatisticViewModel GetSampleData()
        {
            var sut = new SuspiciousStatisticsReportService();

            var list = new List<SuspiciousInfo>
            {
                new SuspiciousInfo
                {
                    CustId = 3293,
                    MatchId = 13760671,
                    CaseId = 4,
                    TaskId = 4
                },
                new SuspiciousInfo
                {
                    CustId = 1280,
                    MatchId = 4032122,
                    CaseId = 4,
                    TaskId = 4
                },
                new SuspiciousInfo
                {
                    CustId = 1280,
                    MatchId = 12768041,
                    CaseId = 4,
                    TaskId = 4
                }
            };

            return sut.GetSuspiciousCustomerStatistics(list);
        }

        private SuspiciousStatisticViewModel GetSampleDataFromArchive()
        {
            var sut = new SuspiciousStatisticsReportService();

            var list = new List<SuspiciousInfo>
            {
                new SuspiciousInfo
                {
                    CustId = 1280,
                    MatchId = 13606626,
                    CaseId = 4,
                    TaskId = 4
                },
                new SuspiciousInfo
                {
                    CustId = 1280,
                    MatchId = 13659022,
                    CaseId = 4,
                    TaskId = 4
                },
                new SuspiciousInfo
                {
                    CustId = 1280,
                    MatchId = 13675244,
                    CaseId = 4,
                    TaskId = 4
                }
            };

            return sut.GetSuspiciousCustomerStatistics(list);
        }
    }
}