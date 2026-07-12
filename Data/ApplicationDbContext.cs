using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using talentacquisition_jobplacement_mvc.Models;

namespace talentacquisition_jobplacement_mvc.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;
        public DbSet<AttributeDefinition> AttributeDefinitions { get; set; } = null!;
        public DbSet<Position> Positions { get; set; } = null!;
        public DbSet<PositionAttribute> PositionAttributes { get; set; } = null!;
        public DbSet<CandidateProfile> CandidateProfiles { get; set; } = null!;
        public DbSet<CandidateProfileAttribute> CandidateProfileAttributes { get; set; } = null!;
        public DbSet<CV> CVs { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<CVLike> CVLikes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Position Attributes
            modelBuilder.Entity<PositionAttribute>()
                .HasOne(pa => pa.Position)
                .WithMany(p => p.PositionAttributes)
                .HasForeignKey(pa => pa.PositionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PositionAttribute>()
                .HasOne(pa => pa.AttributeDefinition)
                .WithMany()
                .HasForeignKey(pa => pa.AttributeDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Candidate Profile
            modelBuilder.Entity<CandidateProfile>()
                .HasOne(cp => cp.User)
                .WithOne()
                .HasForeignKey<CandidateProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CV
            modelBuilder.Entity<CV>()
                .HasOne(cv => cv.Position)
                .WithMany(p => p.CVs)
                .HasForeignKey(cv => cv.PositionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CV>()
                .HasOne(cv => cv.User)
                .WithMany()
                .HasForeignKey(cv => cv.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CV>()
                .HasOne(cv => cv.CandidateProfile)
                .WithMany(cp => cp.CVs)
                .HasForeignKey(cv => cv.CandidateProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Projects
            modelBuilder.Entity<Project>()
                .HasOne(p => p.CandidateProfile)
                .WithMany(cp => cp.Projects)
                .HasForeignKey(p => p.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comments
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.CV)
                .WithMany(cv => cv.Comments)
                .HasForeignKey(c => c.CVId)
                .OnDelete(DeleteBehavior.Cascade);

            // Likes
            modelBuilder.Entity<CVLike>()
                .HasOne(l => l.CV)
                .WithMany(cv => cv.Likes)
                .HasForeignKey(l => l.CVId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}