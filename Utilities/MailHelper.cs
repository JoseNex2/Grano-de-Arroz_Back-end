using System;
using System.IO;
using System.Threading.Tasks;
using Entities.Domain.DTO;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using MimeKit;

namespace Utilities
{
    public interface IMailHelper
    {
        Task SendRecoveryEmailAsync(DataRecoveryDTO dataRecovery, string token);
        Task SendWelcomeEmailAsync(WelcomeEmailDTO welcomeData);
    }

    public class MailHelper : IMailHelper
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MailHelper(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendRecoveryEmailAsync(DataRecoveryDTO dataRecovery, string token)
        {
            try
            {
                string templatePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "email-templates", "recover-password.html"
                );

                string htmlTemplate = await File.ReadAllTextAsync(templatePath);

                string recoveryUrl = $"{dataRecovery.Url}/{token}";

                string htmlBody = htmlTemplate.Replace("{{RECOVER_PASSWORD_LINK}}", recoveryUrl);

                BodyBuilder bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };

                MimeMessage emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("Sistema de recuperación de contraseña", Environment.GetEnvironmentVariable("MAIL_RECOVERY")));
                emailMessage.To.Add(new MailboxAddress("", dataRecovery.Email));
                emailMessage.Subject = "Recuperar contraseña";
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using (SmtpClient client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(Environment.GetEnvironmentVariable("MAIL_RECOVERY"), Environment.GetEnvironmentVariable("MAIL_TOKEN"));
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar el email de recuperación: {ex.Message}", ex);
            }
        }

        public async Task SendWelcomeEmailAsync(WelcomeEmailDTO welcomeData)
        {
            try
            {
                string templatePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "email-templates", "welcome-email.html"
                );

                string htmlTemplate = await File.ReadAllTextAsync(templatePath);

                string welcomeUrl = $"{welcomeData.Url}/{welcomeData.Token}";

                string htmlBody = htmlTemplate
                    .Replace("{{USER_NAME}}", welcomeData.UserName)
                    .Replace("{{USER_ROLE}}", welcomeData.Role)
                    .Replace("{{CREATE_PASSWORD_LINK}}", welcomeUrl);

                BodyBuilder bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };

                MimeMessage emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("Sistema de bienvenida", Environment.GetEnvironmentVariable("MAIL_RECOVERY")));
                emailMessage.To.Add(new MailboxAddress("", welcomeData.Email));
                emailMessage.Subject = "Bienvenido a GDA";
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using (SmtpClient client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(Environment.GetEnvironmentVariable("MAIL_RECOVERY"), Environment.GetEnvironmentVariable("MAIL_TOKEN"));
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar el email de bienvenida: {ex.Message}", ex);
            }
        }
    }
}