using System.ComponentModel.DataAnnotations;

namespace talentacquisition_jobplacement_mvc.Models
{
    public class Position
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;   // e.g. "Business Analyst"

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<PositionAttribute> PositionAttributes { get; set; } = new List<PositionAttribute>();
        public ICollection<CV> CVs { get; set; } = new List<CV>();
    }
}