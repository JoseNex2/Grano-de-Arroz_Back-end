using System;
using System.IO;
using System.Threading.Tasks;
using Entities.Domain.DTO;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using MimeKit;
using MimeKit.Utils;

namespace Utilities
{
    public interface IMailHelper
    {
        Task SendRecoveryEmailAsync(DataRecoveryDTO dataRecovery, string token);
    }

    public class MailHelper : IMailHelper
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MailHelper(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        private string EmbedEmailImages(string htmlBody, BodyBuilder bodyBuilder)
        {
            var imageMatches = System.Text.RegularExpressions.Regex.Matches(
                htmlBody, 
                @"src=""[^""]*?([^/""]+\.svg)""", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            string imagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "emailCard");

            foreach (System.Text.RegularExpressions.Match match in imageMatches)
            {
                if (match.Groups.Count < 2) continue;

                string imageFileName = match.Groups[1].Value;
                string imagePath = Path.Combine(imagesPath, imageFileName);

                if (!File.Exists(imagePath))
                {
                    imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", imageFileName);
                }

                if (!File.Exists(imagePath))
                {
                    string[] possiblePaths = new[]
                    {
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
                }

                if (File.Exists(imagePath))
                {
                    MimeEntity image = bodyBuilder.LinkedResources.Add(imagePath);
                    string contentId = Guid.NewGuid().ToString();
                    image.ContentId = contentId;
                    
                    htmlBody = System.Text.RegularExpressions.Regex.Replace(
                        htmlBody,
                        $@"src=""[^""]*{System.Text.RegularExpressions.Regex.Escape(imageFileName)}""",
                        $"src=\"cid:{contentId}\"");
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
    }
}