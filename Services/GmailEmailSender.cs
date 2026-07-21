using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace talentacquisition_jobplacement_mvc.Services
{
    /// <summary>
    /// Simple SMTP-based IEmailSender implementation.
    /// Falls back to writing email files to disk when SMTP settings are not configured (useful for local/dev).
    /// </summary>
    public class GmailEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<GmailEmailSender> _logger;

        public GmailEmailSender(IOptions<EmailSettings> options, ILogger<GmailEmailSender> logger)
        {
            _settings = options?.Value ?? new EmailSettings();
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(_settings.SmtpHost) ||
                string.IsNullOrWhiteSpace(_settings.SmtpUser) ||
                string.IsNullOrWhiteSpace(_settings.SmtpPass))
            {
                // Fallback behavior: write email to disk so flows still work without an SMTP server.
                try
                {
                    var outFolder = !string.IsNullOrWhiteSpace(_settings.OutputFolder)
                        ? _settings.OutputFolder
                        : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "emails");

                    Directory.CreateDirectory(outFolder);

                    var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmssfff}_{Guid.NewGuid():N}.html";
                    var filePath = Path.Combine(outFolder, fileName);

                    var content = new StringBuilder();
                    content.AppendLine($"To: {email}");
                    content.AppendLine($"Subject: {subject}");
                    content.AppendLine();
                    content.AppendLine(htmlMessage);

                    await File.WriteAllTextAsync(filePath, content.ToString(), Encoding.UTF8);
                    _logger.LogWarning("SMTP not configured. Wrote email to {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to write fallback email to disk.");
                }

                return;
            }

            try
            {
                using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
                {
                    EnableSsl = _settings.UseSsl,
                    Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPass),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 20_000
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(
                        string.IsNullOrWhiteSpace(_settings.FromEmail) ? _settings.SmtpUser : _settings.FromEmail,
                        _settings.FromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                message.To.Add(email);

                await client.SendMailAsync(message);

                _logger.LogInformation("Email sent to {Email} (subject: {Subject})", email, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);

                // As a safety net, write the failed email to disk for later inspection
                try
                {
                    var outFolder = !string.IsNullOrWhiteSpace(_settings.OutputFolder)
                        ? _settings.OutputFolder
                        : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "emails");

                    Directory.CreateDirectory(outFolder);

                    var fileName = $"FAILED_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}_{Guid.NewGuid():N}.html";
                    var filePath = Path.Combine(outFolder, fileName);

                    var content = new StringBuilder();
                    content.AppendLine($"To: {email}");
                    content.AppendLine($"Subject: {subject}");
                    content.AppendLine();
                    content.AppendLine(htmlMessage);
                    content.AppendLine();
                    content.AppendLine($"Error: {ex}");

                    await File.WriteAllTextAsync(filePath, content.ToString(), Encoding.UTF8);
                    _logger.LogWarning("Wrote failed email to {FilePath}", filePath);
                }
                catch (Exception writeEx)
                {
                    _logger.LogError(writeEx, "Failed to write failed email to disk.");
                }
            }
        }
    }
}