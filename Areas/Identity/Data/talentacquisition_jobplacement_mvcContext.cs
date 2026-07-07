using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace talentacquisition_jobplacement_mvc.Data;

public class talentacquisition_jobplacement_mvcContext : IdentityDbContext<IdentityUser>
{
    public talentacquisition_jobplacement_mvcContext(DbContextOptions<talentacquisition_jobplacement_mvcContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
