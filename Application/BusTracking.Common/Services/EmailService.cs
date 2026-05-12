using BusTracking.Common.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace BusTracking.Common.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _cfg;
        public EmailService(IConfiguration cfg) => _cfg = cfg;

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_cfg["Email:SenderName"], _cfg["Email:SenderEmail"]));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(_cfg["Email:Host"], int.Parse(_cfg["Email:Port"] ?? "587"),
                                      MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_cfg["Email:Username"], _cfg["Email:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
