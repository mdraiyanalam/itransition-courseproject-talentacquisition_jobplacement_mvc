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
var connectionString = builder.Configuration.GetConnectionString("talentacquisition_jobplacement_mvcContextConnection")
    ?? throw new InvalidOperationException("Connection string 'talentacquisition_jobplacement_mvcContextConnection' not found.");

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

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
        });
    Console.WriteLine("✅ Google Authentication enabled.");
}
else
{
    Console.WriteLine("⚠️ Google Authentication is disabled.");
}

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

var app = builder.Build();

// QuestPDF License
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Evaluation;

// Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Administrator", "Recruiter", "Candidate" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        await SeedData.Initialize(services);
        Console.WriteLine("✅ Database seeded successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
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

app.Run();