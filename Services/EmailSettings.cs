using System;

namespace talentacquisition_jobplacement_mvc.Services
{
    public class EmailSettings
    {
        // SMTP host (e.g. smtp.gmail.com)
        public string SmtpHost { get; set; } = string.Empty;

        // SMTP port (587 for TLS)
        public int SmtpPort { get; set; } = 587;

        // SMTP username (email)
        public string SmtpUser { get; set; } = string.Empty;

        // SMTP password or app password
        public string SmtpPass { get; set; } = string.Empty;

        // Enable SSL/TLS
        public bool UseSsl { get; set; } = true;

        // From email displayed to recipients. If empty, SmtpUser will be used.
        public string FromEmail { get; set; } = string.Empty;

        // Friendly from name
        public string FromName { get; set; } = "Talent Acquisition";

        // Local output folder used when SMTP is not configured (helpful during development)
        public string OutputFolder { get; set; } = string.Empty;
    }
}