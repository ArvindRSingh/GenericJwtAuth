using System;
using System.Collections.Generic;
using System.Text;
using SendGrid;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Identity;

namespace AzureTableIdentity.Core
{
    public class EmailSender : IEmailSender
    {
        public EmailSender(AuthMessageSenderOptions optionsAccessor)
        {
            Options = optionsAccessor;
        }

        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        public Task SendEmailAsync(string senderEmail, string recepientEmail, string subject, string message)
        {
            return Execute(Options.SendGridKey, subject, message, senderEmail, recepientEmail);
        }

        public Task Execute(string apiKey, string subject, string message, string senderEamil, string recepientEmail)
        {
            //var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(senderEamil, Options.SendGridUser),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(recepientEmail));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            //return client.SendEmailAsync(msg);
            return Task.FromResult<object>(null);
        }
    }

    public interface IEmailSender
    {
        Task SendEmailAsync(string senderEmail, string recepientEmail, string subject, string message);
    }
}
