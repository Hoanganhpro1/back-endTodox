using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using TodoApp.Core.Services;

namespace TodoApp.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string username)
        {
            var appUrl = _configuration["AppUrl"];
            var resetUrl = $"{appUrl}/reset-password?token={resetToken}";

            var subject = "Reset Your Password - TodoApp";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Hello {username},</h2>
                    <p>You requested to reset your password. Click the link below to reset it:</p>
                    <p>
                        <a href='{resetUrl}' 
                           style='background-color: #ec4899; color: white; padding: 12px 24px; 
                                  text-decoration: none; border-radius: 5px; display: inline-block;'>
                            Reset Password
                        </a>
                    </p>
                    <p>This link will expire in 1 hour.</p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <br>
                    <p>Best regards,<br>TodoApp Team</p>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _configuration["Email:SenderName"],
                _configuration["Email:SenderEmail"]
            ));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _configuration["Email:SmtpServer"],
                int.Parse(_configuration["Email:SmtpPort"]!),
                false
            );
            await client.AuthenticateAsync(
                _configuration["Email:SenderEmail"],
                _configuration["Email:Password"]
            );
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}