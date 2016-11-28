// --------------------------------------------------------------------------
// <copyright file="FDMailSender.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.MailSenders
{
    using Fanex.MailDelivery.Core.Models;
    using WinService.FraudDetection.Core.MailMasterRenders;

    public class FDMailSender : IMailSender
    {
        public FDMailSender()
        {
            MailClient = new MailClient();
        }

        public FDMailSender(IMailClient client)
        {
            MailClient = client;
        }

        public IMailMasterRenderAbstractFactory MailMasterRenderAbstractFactory { get; set; }

        public IMailClient MailClient { get; set; }

        public void SendMail(int mailMasterId)
        {
            var mailMasterRender = MailMasterRenderAbstractFactory.GetMailMasterRender();
            var mail = mailMasterRender.RenderContentOfMailMaster(mailMasterId);

            var newMail = new Mail
            {
                Subject = mail.MailSubject,
                Title = mail.MailSubject,
                Body = mail.MailContent,
                MailingListId = mail.MailingListId
            };

            MailClient.Send(newMail);
        }
    }
}