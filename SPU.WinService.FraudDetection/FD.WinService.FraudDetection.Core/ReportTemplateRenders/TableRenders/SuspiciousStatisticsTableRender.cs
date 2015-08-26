// --------------------------------------------------------------------------
// <copyright file="SuspiciousStatisticsTableRender.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.ReportTemplateRenders.TableRenders
{
    using System.Collections.Generic;
    using Lib.Model.ReportTemplate;

    public class SuspiciousStatisticsTableRender : TableRender
    {
        public SuspiciousStatisticsTableRender()
        {
        }

        public SuspiciousStatisticsTableRender(IRazor razor)
            : base(razor)
        {
        }

        public IList<SuspiciousCustormer> Model { get; set; }

        public override string RenderTable()
        {
            var result = _razor.Parse(Template, Model);

            return result;
        }
    }
}