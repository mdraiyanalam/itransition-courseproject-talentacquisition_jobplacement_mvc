using System.ComponentModel.DataAnnotations;

namespace talentacquisition_jobplacement_mvc.Models
{
    public class Position
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Company { get; set; }
        public string? Level { get; set; }           // Junior, Mid, Senior, Lead, etc.
        public string? ProjectTags { get; set; }     // Comma separated
        public int? MaxProjects { get; set; } = 3;

        public string? AllowedRoles { get; set; } = "Candidate";
        public string? AccessRules { get; set; }     // JSON string for advanced rules

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<PositionAttribute> PositionAttributes { get; set; } = new List<PositionAttribute>();
        public ICollection<CV> CVs { get; set; } = new List<CV>();
    }
}