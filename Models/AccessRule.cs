using System.ComponentModel.DataAnnotations;

namespace talentacquisition_jobplacement_mvc.Models
{
    public class AccessRule
    {
        public int AttributeId { get; set; }
        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;

        [Required]
        public int AttributeDefinitionId { get; set; }
        public AttributeDefinition AttributeDefinition { get; set; } = null!;

        [Required]
        public string Operator { get; set; } = "="; // =, !=, >, <, >=, <=

        [Required]
        public string Value { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}