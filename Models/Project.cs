namespace talentacquisition_jobplacement_mvc.Models
{
    public class Project
    {
        public int Id { get; set; }

        public int CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public string? TechnologyTags { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}