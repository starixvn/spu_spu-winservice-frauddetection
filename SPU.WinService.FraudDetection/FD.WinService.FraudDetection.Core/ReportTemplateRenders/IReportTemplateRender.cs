// --------------------------------------------------------------------------
// <copyright file="IReportTemplateRender.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.Lib.ReportTemplateRenders
{
    using System.Collections.Generic;
    using WinService.FraudDetection.Core.ReportTemplateRenders.TableRenders;

    public interface IReportTemplateRender
    {
        IList<ITableRender> TableRenders { get; set; }

        string RenderContent();
    }
}