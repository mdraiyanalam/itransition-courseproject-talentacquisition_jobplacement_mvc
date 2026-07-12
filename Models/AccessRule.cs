namespace talentacquisition_jobplacement_mvc.Models
{
    public class AccessRule
    {
        public int AttributeId { get; set; }
        public string Operator { get; set; } = "=";
        public string Value { get; set; } = string.Empty;
    }
}