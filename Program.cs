using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Hubs;
using talentacquisition_jobplacement_mvc.Models;
using talentacquisition_jobplacement_mvc.Resources;
using talentacquisition_jobplacement_mvc.Services;

var builder = WebApplication.CreateBuilder(args);

// Connection String
//var connectionString = builder.Configuration.GetConnectionString("talentacquisition_jobplacement_mvcContextConnection")
//    ?? throw new InvalidOperationException("Connection string 'talentacquisition_jobplacement_mvcContextConnection' not found.");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("talentacquisition_jobplacement_mvcContextConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

// DbContext
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));

// DbContext - Use PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

Console.WriteLine("===== CONNECTION STRING =====");
Console.WriteLine(string.IsNullOrEmpty(connectionString) ? "EMPTY / NULL" : connectionString);
Console.WriteLine("=============================");

// ========== DATA PROTECTION (FIX FOR CORRELATION FAILED) ==========
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "keys");
Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("TalentAcquisitionApp");
// ==================================================================

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Google Auth
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            options.CallbackPath = "/signin-google";
            options.SaveTokens = true;
            options.CorrelationCookie.SameSite = SameSiteMode.None;
            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        });
    Console.WriteLine("✅ Google Authentication enabled.");
}
else
{
    Console.WriteLine("⚠️ Google Authentication is disabled.");
}

// ========== COOKIE POLICY (VERY IMPORTANT ON RENDER) ==========
builder.Services.ConfigureExternalCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Localization - resources path
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// MVC + Services + SignalR (single AddControllersWithViews call with localization)
builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResources));
    });

builder.Services.AddScoped<CVGeneratorService>();
builder.Services.AddSignalR();

// Email settings binding
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Email Sender
builder.Services.AddSingleton<IEmailSender, GmailEmailSender>();

// Startup-time check for SMTP configuration (do not log secrets)
{
    var emailSection = builder.Configuration.GetSection("EmailSettings");
    var smtpHost = emailSection["SmtpHost"];
    var smtpUser = emailSection["SmtpUser"];
    var smtpPass = emailSection["SmtpPass"];

    if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
    {
        Console.WriteLine("⚠️ Email SMTP is not fully configured. The app will fall back to writing emails to wwwroot/emails.\nSet EmailSettings:SmtpHost, EmailSettings:SmtpUser and EmailSettings:SmtpPass using user-secrets or environment variables.");
    }
    else
    {
        Console.WriteLine($"✅ SMTP configured (host: {smtpHost}). Emails will be sent via SMTP.");
    }
}

var app = builder.Build();

// QuestPDF License
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Evaluation;

// Apply migrations + Seed data on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();

        Console.WriteLine(">>> Dropping and recreating database...");
        db.Database.EnsureDeleted();   // Deletes everything
        db.Database.EnsureCreated();   // Creates all tables from the current model
        Console.WriteLine(">>> Database recreated successfully");

        // Seed Roles
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Administrator", "Recruiter", "Candidate" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                Console.WriteLine($">>> Role created: {roleName}");
            }
        }

        await SeedData.Initialize(services);
        Console.WriteLine(">>> Database seeded successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine(">>> ERROR: " + ex.Message);
        Console.WriteLine(ex.ToString());
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Localization Middleware — call early, before routing/auth
var supportedCultures = new[] { "en", "uz" };
var cultureInfos = supportedCultures.Select(c => new CultureInfo(c)).ToList();

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = cultureInfos,
    SupportedUICultures = cultureInfos,
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        // prefer query string (for quick switching), then cookie, then accept-language
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    }
};

app.UseRequestLocalization(localizationOptions);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Routes
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<DiscussionHub>("/discussionHub");

// Google Logout Fix
app.Use(async (context, next) =>
{
    await next();
    if (context.Request.Path.StartsWithSegments("/Identity/Account/Logout") && context.Response.StatusCode == 200)
    {
        context.Response.Redirect("/");
    }
});

// Create Upload Directories
var uploadsRoot = Path.Combine(app.Environment.WebRootPath, "uploads");
Directory.CreateDirectory(Path.Combine(uploadsRoot, "profile-photos"));
Directory.CreateDirectory(Path.Combine(uploadsRoot, "attributes"));

// Lightweight health endpoint for email readiness (dev-safe)
app.MapGet("/health/email", (IConfiguration config) =>
{
    var section = config.GetSection("EmailSettings");
    var host = section["SmtpHost"];
    var user = section["SmtpUser"];
    var pass = section["SmtpPass"];

    return Results.Json(new
    {
        ready = !string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass),
        host = string.IsNullOrWhiteSpace(host) ? null : host,
        configuredUser = string.IsNullOrWhiteSpace(user) ? (string?)null : (user.Contains("@") ? user : null)
    });
});

app.Run();