using Entities.Domain.DTO;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using MimeKit;

namespace DataAccess.SupportServices
{
    public interface IMailService
    {
        Task SendRecoveryEmailAsync(DataRecoveryDTO dataRecovery, string token);
        Task SendWelcomeEmailAsync(WelcomeEmailDTO welcomeData);
    }

    public class MailService : IMailService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MailService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        private string EmbedEmailImages(string htmlBody, BodyBuilder bodyBuilder)
        {
            // Buscar todas las imágenes (svg, png, jpg, jpeg, gif) en atributos src
            var imageMatches = System.Text.RegularExpressions.Regex.Matches(
                htmlBody, 
                @"src=""([^""]+)""", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            string imagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "emailCard");

            foreach (System.Text.RegularExpressions.Match match in imageMatches)
            {
                if (match.Groups.Count < 2) continue;

                string imageSrc = match.Groups[1].Value;
                
                // Si ya es un CID (Content-ID), saltar
                if (imageSrc.StartsWith("cid:", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Extraer el nombre del archivo de la ruta
                string imageFileName = Path.GetFileName(imageSrc);
                
                // Buscar el archivo en diferentes ubicaciones posibles
                string imagePath = null;
                string[] possiblePaths = new[]
                {
                    Path.Combine(imagesPath, imageFileName),
                    Path.Combine(_webHostEnvironment.WebRootPath, "images", imageFileName),
                    Path.Combine(_webHostEnvironment.WebRootPath, imageFileName),
                    Path.Combine(_webHostEnvironment.WebRootPath, "emailCard", imageFileName)
                };

                foreach (string possiblePath in possiblePaths)
                {
                    if (File.Exists(possiblePath))
                    {
                        imagePath = possiblePath;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    try
                    {
                        MimeEntity image = bodyBuilder.LinkedResources.Add(imagePath);
                        string contentId = Guid.NewGuid().ToString();
                        image.ContentId = contentId;
                        
                        // Reemplazar la ruta original con el Content-ID
                        htmlBody = htmlBody.Replace(
                            $"src=\"{imageSrc}\"",
                            $"src=\"cid:{contentId}\"");
                    }
                    catch
                    {
                        // Si falla al agregar la imagen, continuar con la siguiente
                        continue;
                    }
                }
            }

            return htmlBody;
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

                BodyBuilder bodyBuilder = new BodyBuilder();

                htmlBody = EmbedEmailImages(htmlBody, bodyBuilder);

                bodyBuilder.HtmlBody = htmlBody;

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

                BodyBuilder bodyBuilder = new BodyBuilder();

                htmlBody = EmbedEmailImages(htmlBody, bodyBuilder);

                bodyBuilder.HtmlBody = htmlBody;

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