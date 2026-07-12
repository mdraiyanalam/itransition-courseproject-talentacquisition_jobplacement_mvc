using System.ComponentModel.DataAnnotations;

namespace talentacquisition_jobplacement_mvc.Models
{
    public class AttributeDefinition
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "General";

        [Required]
        public string Type { get; set; } = "Text";

        public string? Options { get; set; }

        public bool IsRequired { get; set; } = true;

        [StringLength(500)]
        public string? Description { get; set; }

        public string? Icon { get; set; }
    }
}