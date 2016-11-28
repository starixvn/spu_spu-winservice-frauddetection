// --------------------------------------------------------------------------
// <copyright file="IMailClient.cs" company="Nexcel Solutions Vietnam">
// Copyright (coffee) Nexcel Solutions Vietnam. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------
// <history>
//   <change who="Long.Luu" date="2015.07.23">Created</change>
// </history>

namespace FD.WinService.FraudDetection.Core.MailSenders
{
    using Fanex.MailDelivery.Client;
    using Fanex.MailDelivery.Core.Models;

    public interface IMailClient
    {
        void Send(Mail mail);
    }

    public class MailClient : IMailClient
    {
        public void Send(Mail mail)
        {
            MailClientHandler.SendFunctionMail(mail);
        }
    }
}