# Talent Acquisition and Job Placement Platform

A modern, full-featured ASP.NET Core web application for managing job positions, candidate profiles, and CV submissions.

## ✨ Features

- **Role-based Access**: Administrator, Recruiter, Candidate
- **Dynamic Attributes**: Flexible, configurable candidate data fields
- **CV Management**: Create, edit, publish, and download PDF CVs
- **Google Authentication** + Email Confirmation
- **Real-time Discussions** using SignalR
- **PDF Generation** with QuestPDF
- **Multi-language Support** (English + Uzbek)
- **Admin Panel** with user/role management
- **Responsive UI** with Bootstrap 5

## 🚀 Quick Start

### Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (or SQL Server)

### Installation

1. Clone the repository
2. Update connection string in `appsettings.json`
3. Run the following commands:

```bash
dotnet restore
dotnet ef database update
dotnet run
```

Default Admin Login:

Email: admin@talentacquisition.local

Password: Admin@123

---

## Email (SMTP) configuration

The application uses a typed `EmailSettings` class and a simple SMTP-backed `GmailEmailSender`.
If SMTP credentials are not provided the app falls back to writing email HTML files to `wwwroot/emails` (useful for local development).

Recommended: store SMTP credentials securely with `dotnet user-secrets` during development.

1) Initialize user-secrets (run from project folder):

```bash
dotnet user-secrets init
```

2) Set SMTP settings (example for Gmail using App Password):

```bash
dotnet user-secrets set "EmailSettings:SmtpHost" "smtp.gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPort" "587"
dotnet user-secrets set "EmailSettings:SmtpUser" "you@gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPass" "<your-16-char-app-password>"
dotnet user-secrets set "EmailSettings:FromName" "Talent Acquisition"
dotnet user-secrets set "EmailSettings:FromEmail" "you@gmail.com"
```

Note: in production prefer environment variables (use `EmailSettings__SmtpHost`, etc.).

Program.cs performs a startup check and will log a warning when SMTP settings are missing. If missing, emails will be written to `wwwroot/emails`.

---

## Project Structure

```
/ (project root)
├── Areas/Identity/          # ASP.NET Identity Pages
├── Controllers/             # MVC Controllers
├── Data/                    # DbContext + Migrations + SeedData
├── Models/                  # Domain models
├── Services/                # Business services (CVGenerator, Email)
├── Views/                   # Razor Views
├── wwwroot/                 # Static files
└── Program.cs
```

## Development notes

- Use MailKit or system SMTP credentials as configured above.
- The app includes a fallback email writer for development (see `wwwroot/emails`).
- Tag input on Projects uses Tagify (loaded from CDN in `Views/Profiles/Projects.cshtml`).

---

If you want, I can also:

- Add a short `docs/email.md` describing how to generate a Gmail App Password and test the flow.
- Add CI step that fails the build if required environment variables are missing (for production).

