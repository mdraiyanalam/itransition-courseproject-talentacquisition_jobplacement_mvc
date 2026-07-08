using System.ComponentModel.DataAnnotations;

namespace talentacquisition_jobplacement_mvc.Models
{
    public class AttributeDefinition
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;   // e.g. "English Level", "GPA", "Years of Experience"

        [Required]
        public string Type { get; set; } = string.Empty;   // Text, Number, Dropdown, Date, Boolean

        public string? Options { get; set; }               // JSON string for dropdown options

        public bool IsRequired { get; set; } = true;
        public string? Description { get; set; }
    }
}