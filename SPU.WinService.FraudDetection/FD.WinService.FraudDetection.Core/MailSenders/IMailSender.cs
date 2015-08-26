// --------------------------------------------------------------------------
// <copyright file="IMailSender.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.MailSenders
{
    using WinService.FraudDetection.Core.MailMasterRenders;

    public interface IMailSender
    {
        IMailMasterRenderAbstractFactory MailMasterRenderAbstractFactory { get; set; }

        IMailClient MailClient { get; set; }

        void SendMail(int mailMasterId);
    }
}