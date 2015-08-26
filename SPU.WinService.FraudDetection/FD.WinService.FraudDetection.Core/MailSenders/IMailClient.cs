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
    using Fanex.MailDelivery.Client.Handlers;
    using Fanex.MailDelivery.Client.Models;

    public interface IMailClient
    {
        void Send(Mail mail);
    }

    public class MailClient : IMailClient
    {
        public void Send(Mail mail)
        {
            MailClientHandler.SendMail(mail);         
        }
    }
}