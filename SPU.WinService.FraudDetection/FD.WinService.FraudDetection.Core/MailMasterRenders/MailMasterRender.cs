// --------------------------------------------------------------------------
// <copyright file="MailMasterRender.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.MailMasterRenders
{
    using Fanex.Data.Repository;
    using Lib.Criteria.MailMaster;
    using Lib.DTO;
    using Lib.ReportTemplateRenders;
    using Lib.Service.Implement;
    using Lib.Service.Interface;

    public class MailMasterRender : IMailMasterRender
    {
        public MailMasterRender(IReportRenderAbstractFactory renderAbstractFactory)
        {
            MailMasterService = new MailMasterService();
            PlaceholderReplacer = new PlaceholderReplacer();
            ReportRenderAbstractFactory = renderAbstractFactory;
        }

        public MailMasterRender(
            IMailMasterService mailMasterService,
            IReportRenderAbstractFactory renderAbstractFactory,
            IPlaceholderReplacer placeholderReplacer)
        {
            MailMasterService = mailMasterService;
            PlaceholderReplacer = placeholderReplacer;
            ReportRenderAbstractFactory = renderAbstractFactory;
        }

        public IMailMasterService MailMasterService { get; set; }

        public IReportRenderAbstractFactory ReportRenderAbstractFactory { get; set; }

        public IPlaceholderReplacer PlaceholderReplacer { get; set; }

        public MailMasterInfo RenderContentOfMailMaster(int mailMasterId)
        {
            ICriteria criteria = new MailMasterGetByIdCriteria { MailId = mailMasterId };
            var mailMaster = MailMasterService.GetMailById(criteria);
            var render = ReportRenderAbstractFactory.GetReportTemplateRender();
            var contentOfPlaceholder = render.RenderContent();

            var newMailContent = PlaceholderReplacer.ReplacePlaceholder(mailMaster.MailContent, contentOfPlaceholder);
            mailMaster.MailContent = newMailContent;

            return mailMaster;
        }
    }
}