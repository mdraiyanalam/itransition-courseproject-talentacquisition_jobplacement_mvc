namespace talentacquisition_jobplacement_mvc.Models
{
    public class UserAchievement
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public DateTime AwardedAt { get; set; } = DateTime.UtcNow;
    }
}