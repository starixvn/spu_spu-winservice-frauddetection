// --------------------------------------------------------------------------
// <copyright file="FDRazor.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.ReportTemplateRenders.TableRenders
{
    using RazorEngine;

    public class FDRazor : IRazor
    {
        public string Parse(string template, object model)
        {
            return Razor.Parse(template, model);
        }
    }
}