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

---

## 🔧 Configuration & Setup

### User Secrets (Local Development)

Store sensitive configuration securely using \dotnet user-secrets\:

#### Initialize User Secrets

\\\ash
dotnet user-secrets init
\\\

This creates a \.gitignore\'d secrets file in \%APPDATA%\Microsoft\UserSecrets\<project-id>\secrets.json\ (Windows) or \~/.microsoft/usersecrets/<project-id>/secrets.json\ (macOS/Linux).

#### OAuth / Social Login Setup (Optional)

##### Google OAuth

1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Create a new project
3. Enable the Google+ API
4. Create OAuth 2.0 credentials (Web application)
5. Add redirect URI: \https://localhost:5001/signin-google\ (or your domain)
6. Store the Client ID and Secret:

\\\ash
dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-client-secret"
\\\

### Email (SMTP) Configuration


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

### Database Setup

The application uses **Entity Framework Core** with SQL Server.

#### Create/Update Database

Run migrations to set up the database schema and seed demo data:

\\\ash
dotnet ef database update
\\\

This will:
- Create the database (if it doesn't exist)
- Apply all pending migrations
- Seed demo accounts, positions, attributes, and sample data

#### Reset Database (Development)

To delete and recreate the database:

\\\ash
dotnet ef database drop --force
dotnet ef database update
\\\

### Running the Application

\\\ash
dotnet run
\\\

The app will start on \https://localhost:5001\ by default.

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






