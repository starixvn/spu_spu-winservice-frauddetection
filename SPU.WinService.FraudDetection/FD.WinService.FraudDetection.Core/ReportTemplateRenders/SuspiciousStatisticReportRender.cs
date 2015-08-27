// --------------------------------------------------------------------------
// <copyright file="SuspiciousStatisticReportRender.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.ReportTemplateRenders
{
    using System.Collections.Generic;
    using System.Text;
    using Lib.ReportTemplateRenders;
    using WinService.FraudDetection.Core.ReportTemplateRenders.TableRenders;

    public class SuspiciousStatisticReportRender : IReportTemplateRender
    {
        public IList<ITableRender> TableRenders { get; set; }

        public string RenderContent()
        {
            var builder = new StringBuilder();

            foreach (var tableRender in TableRenders)
            {
                var tableHtml = tableRender.RenderTable();
                builder.Append(tableHtml);
                builder.Append("<p/>");
            }

            var result = builder.ToString();

            return result;
        }
    }
}