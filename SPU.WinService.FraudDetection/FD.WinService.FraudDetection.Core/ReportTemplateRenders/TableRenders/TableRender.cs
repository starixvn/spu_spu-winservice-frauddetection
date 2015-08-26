// --------------------------------------------------------------------------
// <copyright file="TableRender.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.ReportTemplateRenders.TableRenders
{
    using System.Diagnostics.CodeAnalysis;

    public abstract class TableRender : ITableRender
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        protected IRazor _razor;

        protected TableRender()
        {
            _razor = new FDRazor();
        }

        protected TableRender(IRazor razor)
        {
            _razor = razor;
        }

        public string Template { get; set; }

        public abstract string RenderTable();
    }
}