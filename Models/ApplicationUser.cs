using Microsoft.AspNetCore.Identity;

namespace talentacquisition_jobplacement_mvc.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string? FullName { get; set; }

        [PersonalData]
        public string? ProfilePhotoUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsBlocked { get; set; } = false;

        public DateTime? BlockedAt { get; set; }

        public DateTime? UnblockedAt { get; set; }
    }
}