namespace FD.Lib.BusinessManager.UnitTest.Mock_Objects
{
    using System.Net.Mail;
    using Fanex.MailDelivery.Client.Models;
    using Lib.MailSenders;

    public class FakeMailClient : IMailClient
    {
        private readonly SmtpClient _client = new SmtpClient { Host = "127.0.0.1" };

        public void Send(Mail mail)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("from@spu.com")
            };

            mailMessage.To.Add(new MailAddress("to@spu.com"));

            mailMessage.Subject = mail.Subject;
            mailMessage.Body = mail.Body;

            _client.Send(mailMessage);
        }
    }
}