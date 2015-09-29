// --------------------------------------------------------------------------
// <copyright file="SendingEmailNextAction.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.NextActions
{
    using System.Collections.Generic;
    using Lib.BusinessManager.ReportTemplateRenders;
    using Lib.DTO;
    using Lib.Service.Implement;
    using Lib.Service.Interface;
    using WinService.FraudDetection.Core.MailMasterRenders;
    using WinService.FraudDetection.Core.MailSenders;

    /// <summary>
    ///     ActionSendEmail Class.
    /// </summary>
    public class SendingEmailNextAction : INextAction
    {
        public SendingEmailNextAction(int mailMasterId)
        {
            MailMasterId = mailMasterId;
            MailSender = new FDMailSender();
            ReportService = new SuspiciousStatisticsReportService();
        }

        public SendingEmailNextAction(int mailMasterId, IMailSender mailSender, IReportService reportService)
        {
            MailMasterId = mailMasterId;
            MailSender = mailSender;
            ReportService = reportService;
        }

        public int MailMasterId { get; set; }

        public IList<SuspiciousInfo> SuspicionList { get; set; }

        public IList<DataCapture> DataCaptureList { get; set; }

        public IMailSender MailSender { get; set; }

        public IReportService ReportService { get; set; }

        public void TakeAction()
        {
            if (SuspicionList.Count <= 0)
            {
                return;
            }

            var model = ReportService.GetSuspiciousCustomerStatistics(SuspicionList);

            var renderFactory = new SuspiciousStatisticReporRenderAbstractFactory
            {
                SuspiciousStatisticViewModel = model
            };

            var mailMasterRenderAbstractFactory = new MailMasterRenderAbstractFactory
            {
                ReportRenderFactory = renderFactory
            };

            MailSender.MailMasterRenderAbstractFactory = mailMasterRenderAbstractFactory;
            MailSender.SendMail(MailMasterId);
        }
    }
}