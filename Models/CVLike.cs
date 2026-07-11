namespace talentacquisition_jobplacement_mvc.Models
{
    public class CVLike
    {
        public int Id { get; set; }

        public int CVId { get; set; }
        public CV CV { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}