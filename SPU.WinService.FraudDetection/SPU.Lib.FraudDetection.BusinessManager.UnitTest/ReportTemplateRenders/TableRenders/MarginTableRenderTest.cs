namespace SPU.Lib.FraudDetection.BusinessManager.UnitTest.ReportTemplateRenders.TableRenders
{
    using System.Collections.Generic;
    using System.IO;
    using FD.Lib.BusinessManager.ReportTemplateRenders.TableRenders;
    using FD.Lib.Model.ReportTemplate;
    using FD.Lib.ReportTemplateRenders;
    using FD.Lib.ReportTemplateRenders.TableRenders;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class MarginTableRenderTest
    {
        private const string TemplateFilePath = TemplateFileManager.MarginTemplateFile;

        [Test]
        public void When_Call_RenderTable_Then_Method_Parse_Should_Be_Called()
        {
            // ---- Arrange ---
            var mockRazor = new Mock<IRazor>();
            var sut = new MarginTableRender(mockRazor.Object);

            // --- Act ---
            sut.RenderTable();

            // ---- Assert ---
            mockRazor.Verify(x => x.Parse(It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(1));
        }

        [Test]
        public void When_Two_Customers_Are_Inputted_Then_Result_Should_Contain_These_Customers()
        {
            // ---- Arrange ---
            var template = File.ReadAllText(TemplateFilePath);

            var suspiciousCustomers = GetSampleMarginCustormers();

            var sut = new MarginTableRender
            {
                Template = template,
                Model = suspiciousCustomers
            };

            // --- Act ---
            var actualResult = sut.RenderTable();

            // ---- Assert ---
            Assert.IsTrue(actualResult.Contains("member03"));
            Assert.IsTrue(actualResult.Contains("member04"));
        }

        [Test]
        public void When_No_Suspicious_Customers_Are_Inputted_Then_Result_Should_Contain_Warning_Message()
        {
            // ---- Arrange ---
            var template = File.ReadAllText(TemplateFilePath);

            var suspiciousCustomers = new List<MarginCustormer>();

            var sut = new MarginTableRender
            {
                Template = template,
                Model = suspiciousCustomers
            };

            // --- Act ---
            var actualResult = sut.RenderTable();

            // ---- Assert ---
            Assert.IsTrue(actualResult.Contains("No data is available."));
        }

        private List<MarginCustormer> GetSampleMarginCustormers()
        {
            var sampleCustomer1 = new MarginCustormer
            {
                Member = "member03",
                Margins = new List<decimal?> { 0.98m, 1.23m, 3.50m }
            };

            var sampleCustomer2 = new MarginCustormer
            {
                Member = "member04",
                Margins = new List<decimal?> { 1.98m, 12.23m, 32.50m }
            };

            var suspiciousCustomers = new List<MarginCustormer>
            {
                sampleCustomer1,
                sampleCustomer2
            };

            return suspiciousCustomers;
        }
    }
}