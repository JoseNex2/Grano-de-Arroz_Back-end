using Entities.Domain.DTO;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using MimeKit;

namespace DataAccess.SupportServices
{
    public interface IMailService
    {
        Task SendRecoveryEmailAsync(string email, string url);
        Task SendWelcomeEmailAsync(WelcomeEmailDTO welcomeData);
    }

    public class MailService : IMailService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MailService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendRecoveryEmailAsync(string email, string url)
        {
            try
            {
                string templatePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "email-templates", "recover-password.html"
                );

                string htmlTemplate = await File.ReadAllTextAsync(templatePath);

                string htmlBody = htmlTemplate.Replace("{{RECOVER_PASSWORD_LINK}}", url);

                BodyBuilder bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };

                MimeMessage emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("Sistema de recuperación de contraseña", Environment.GetEnvironmentVariable("MAIL_RECOVERY")));
                emailMessage.To.Add(new MailboxAddress("", email));
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

                string htmlBody = htmlTemplate
                    .Replace("{{USER_NAME}}", welcomeData.UserName)
                    .Replace("{{USER_ROLE}}", welcomeData.Role)
                    .Replace("{{CREATE_PASSWORD_LINK}}", welcomeData.Url);

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