using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using talentacquisition_jobplacement_mvc.Data;
using talentacquisition_jobplacement_mvc.Models;
using talentacquisition_jobplacement_mvc.Services;

var builder = WebApplication.CreateBuilder(args);

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("talentacquisition_jobplacement_mvcContextConnection")
    ?? throw new InvalidOperationException("Connection string 'talentacquisition_jobplacement_mvcContextConnection' not found.");

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ==================== Google Authentication ====================
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
    Console.WriteLine("⚠️ Google Authentication is disabled (ClientId/Secret not found in configuration).");
}

// MVC Services
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<CVGeneratorService>();

var app = builder.Build();

// QuestPDF License
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Evaluation;

// Seeding Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services);
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
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Fix Google Login Logout Redirect
app.Use(async (context, next) =>
{
    await next();

    if (context.Request.Path.StartsWithSegments("/Identity/Account/Logout") &&
        context.Response.StatusCode == 200)
    {
        context.Response.Redirect("/");
    }
});

app.Run();