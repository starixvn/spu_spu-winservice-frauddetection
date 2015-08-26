namespace SPU.Lib.FraudDetection.BusinessManager.UnitTest.ReportTemplateRenders.TableRenders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FD.Lib.BusinessManager.ReportTemplateRenders.TableRenders;
    using FD.Lib.Model.ReportTemplate;
    using FD.Lib.ReportTemplateRenders.TableRenders;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SuspiciousStatisticsTableRenderTest
    {
        private const string TemplateFilePath =
            @"..\..\..\SPU.Lib.FraudDetection\ReportTemplateRenders\TableTemplates\SuspiciousStatisticsReport.cshtml";

        [Test]
        public void When_Call_RenderTable_Then_Method_Parse_Should_Be_Called()
        {
            // ---- Arrange ---
            var mockRazor = new Mock<IRazor>();
            var sut = new SuspiciousStatisticsTableRender(mockRazor.Object);

            // --- Act ---
            sut.RenderTable();

            // ---- Assert ---
            mockRazor.Verify(x => x.Parse(It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(1));
        }

        [Test]
        public void When_Two_Suspicious_Customers_Are_Inputted_Then_Result_Should_Contain_These_Customers()
        {
            // ---- Arrange ---
            var template = File.ReadAllText(TemplateFilePath);

            var suspiciousCustomers = GetSampleSuspiciousCustormers();

            var sut = new SuspiciousStatisticsTableRender
            {
                Template = template,
                Model = suspiciousCustomers
            };

            // --- Act ---
            var actualResult = sut.RenderTable();

            // ---- Assert ---
            Assert.IsTrue(actualResult.Contains("member01"));
            Assert.IsTrue(actualResult.Contains("member02"));
        }

        [Test]
        public void When_No_Suspicious_Customers_Are_Inputted_Then_Result_Should_Contain_Warning_Message()
        {
            // ---- Arrange ---
            var template = File.ReadAllText(TemplateFilePath);

            var suspiciousCustomers = new List<SuspiciousCustormer>();

            var sut = new SuspiciousStatisticsTableRender
            {
                Template = template,
                Model = suspiciousCustomers
            };

            // --- Act ---
            var actualResult = sut.RenderTable();

            // ---- Assert ---
            Assert.IsTrue(actualResult.Contains("No data is available."));
        }

        private List<SuspiciousCustormer> GetSampleSuspiciousCustormers()
        {
            var sampleCustomer1 = new SuspiciousCustormer
            {
                Member = "member01",
                Case = "WF01",
                Sport = "Soccer",
                League = "English Premier League",
                MatchId = 123456,
                MatchCode = "2345",
                EventDate = DateTime.Parse("6/13/2015"),
                Event = "Manchester United vs Chelsea"
            };

            var sampleCustomer2 = new SuspiciousCustormer
            {
                Member = "member02",
                Case = "WF01",
                Sport = "Soccer",
                League = "English Premier League",
                MatchId = 123456,
                MatchCode = "2345",
                EventDate = DateTime.Parse("6/13/2015"),
                Event = "Manchester United vs Chelsea"
            };

            var suspiciousCustomers = new List<SuspiciousCustormer>
            {
                sampleCustomer1,
                sampleCustomer2
            };

            return suspiciousCustomers;
        }
    }
}