namespace talentacquisition_jobplacement_mvc.Models.ViewModels
{
    public class OdooPositionDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Company { get; set; }
        public string? ProjectTags { get; set; }
        public string? Location { get; set; }
        public string? EmploymentType { get; set; }
    }
}