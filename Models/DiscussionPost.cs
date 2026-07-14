using System.ComponentModel.DataAnnotations;

namespace talentacquisition_jobplacement_mvc.Models
{
    public class DiscussionPost
    {
        public int Id { get; set; }

        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public string Message { get; set; } = string.Empty; // Markdown supported

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}