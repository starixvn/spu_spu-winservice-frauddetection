// --------------------------------------------------------------------------
// <copyright file="IRazor.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.ReportTemplateRenders.TableRenders
{
    public interface IRazor
    {
        string Parse(string template, object model);
    }
}