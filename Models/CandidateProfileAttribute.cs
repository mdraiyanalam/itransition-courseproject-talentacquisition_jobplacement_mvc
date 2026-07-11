namespace talentacquisition_jobplacement_mvc.Models
{
    public class CandidateProfileAttribute
    {
        public int Id { get; set; }

        public int CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public int AttributeDefinitionId { get; set; }
        public AttributeDefinition AttributeDefinition { get; set; } = null!;

        public string Value { get; set; } = string.Empty;
    }
}