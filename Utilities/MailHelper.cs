using System;
using System.IO;
using System.Threading.Tasks;
using Entities.Domain.DTO;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Utilities
{
    public interface IMailHelper
    {
        Task SendRecoveryEmailAsync(DataRecoveryDTO dataRecovery, string token);
    }

    public class MailHelper : IMailHelper
    {
        public async Task SendRecoveryEmailAsync(DataRecoveryDTO dataRecovery, string token)
        {
            try
            {
                // Ruta directa a la plantilla HTML desde el directorio raíz
                string templatePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Entities", "Domain", "Template", "Mail", "email-templates", "recover-password.html"
                );

                // Leer la plantilla HTML
                string htmlTemplate = await File.ReadAllTextAsync(templatePath);

                // Construir la URL completa de recuperación
                string recoveryUrl = $"{dataRecovery.Url}/{token}";

                // Reemplazar el placeholder con la URL real
                string htmlBody = htmlTemplate.Replace("{{RECOVER_PASSWORD_LINK}}", recoveryUrl);

                // Crear el mensaje de email
                MimeMessage emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("Sistema de recuperación de contraseña", Environment.GetEnvironmentVariable("MAIL_RECOVERY")));
                emailMessage.To.Add(new MailboxAddress("", dataRecovery.Email));
                emailMessage.Subject = "Recuperar contraseña";
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = htmlBody
                };

                // Enviar el email
                using (SmtpClient client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(Environment.GetEnvironmentVariable("MAIL_RECOVERY"), Environment.GetEnvironmentVariable("MAIL_PASSWORD"));
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar el email de recuperación: {ex.Message}", ex);
            }
        }
    }
}