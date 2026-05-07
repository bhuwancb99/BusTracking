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
            // Validate essential configuration values to avoid passing null to MailboxAddress/SMTP methods.
            var senderName = _cfg["Email:SenderName"];
            var senderEmail = _cfg["Email:SenderEmail"] ?? throw new InvalidOperationException("Configuration value 'Email:SenderEmail' is missing.");
            var host = _cfg["Email:Host"] ?? throw new InvalidOperationException("Configuration value 'Email:Host' is missing.");
            var portStr = _cfg["Email:Port"] ?? "587";
            if (!int.TryParse(portStr, out var port)) port = 587;
            var username = _cfg["Email:Username"];
            var password = _cfg["Email:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);

            // Authenticate only when both username and password are provided.
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                await client.AuthenticateAsync(username, password);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

    /*
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
        message.Body    = new TextPart("html") { Text = htmlBody };
 
        using var client = new SmtpClient();
        await client.ConnectAsync(_cfg["Email:Host"], int.Parse(_cfg["Email:Port"] ?? "587"),
                                  MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_cfg["Email:Username"], _cfg["Email:Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
     */
}
