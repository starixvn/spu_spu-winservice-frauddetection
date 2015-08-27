// --------------------------------------------------------------------------
// <copyright file="SuspiciousStatisticReporRenderAbstractFactory.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.Lib.BusinessManager.ReportTemplateRenders
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Lib.ReportTemplateRenders;
    using Model.ReportTemplate;
    using WinService.FraudDetection.Core.ReportTemplateRenders;
    using WinService.FraudDetection.Core.ReportTemplateRenders.TableRenders;

    public class SuspiciousStatisticReporRenderAbstractFactory : IReportRenderAbstractFactory
    {
        public SuspiciousStatisticReporRenderAbstractFactory()
        {
            SuspiciousStatisticViewModel = new SuspiciousStatisticViewModel();
        }

        public SuspiciousStatisticViewModel SuspiciousStatisticViewModel { get; set; }

        public string CurrentDirectory
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public IReportTemplateRender GetReportTemplateRender()
        {
            var suspiciousTableRender = new SuspiciousStatisticsTableRender
            {
                Model = SuspiciousStatisticViewModel.SuspiciousCustormers,
                Template = File.ReadAllText(CurrentDirectory + TemplateFileManager.SuspiciousStatisticTemplateFile)
            };

            var marginTableRender = new MarginTableRender
            {
                Model = SuspiciousStatisticViewModel.MarginCustormers,
                Template = File.ReadAllText(CurrentDirectory + TemplateFileManager.MarginTemplateFile)
            };

            return new SuspiciousStatisticReportRender
            {
                TableRenders = new List<ITableRender>
                {
                    suspiciousTableRender,
                    marginTableRender
                }
            };
        }
    }
}