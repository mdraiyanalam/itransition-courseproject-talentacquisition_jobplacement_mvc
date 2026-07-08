namespace talentacquisition_jobplacement_mvc.Models
{
    public class PositionAttribute
    {
        public int Id { get; set; }

        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;

        public int AttributeDefinitionId { get; set; }
        public AttributeDefinition AttributeDefinition { get; set; } = null!;

        public int Order { get; set; }   // For display order
    }
}