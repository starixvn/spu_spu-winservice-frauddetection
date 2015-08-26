namespace FD.Lib.BusinessManager.UnitTest.ReportTemplateRenders
{
    using System.Collections.Generic;
    using Lib.ReportTemplateRenders;
    using Lib.ReportTemplateRenders.TableRenders;
    using Moq;

    public class FakeSuspiciousStatisticReporRenderAbstractFactory : IReportRenderAbstractFactory
    {
        private const string ContentOfMarginTable = "It's the content of margin table.";
        private const string ContentOfSuspiciousStatisticTable = "It's the content of suspicious statistic table.";

        public IReportTemplateRender GetReportTemplateRender()
        {
            var mockTableRender1 = new Mock<ITableRender>();
            var mockTableRender2 = new Mock<ITableRender>();
            mockTableRender1.Setup(x => x.RenderTable()).Returns(ContentOfMarginTable);
            mockTableRender2.Setup(x => x.RenderTable()).Returns(ContentOfSuspiciousStatisticTable);

            return new SuspiciousStatisticReportRender
            {
                TableRenders = new List<ITableRender>
                {
                    mockTableRender1.Object,
                    mockTableRender2.Object
                }
            };
        }
    }
}