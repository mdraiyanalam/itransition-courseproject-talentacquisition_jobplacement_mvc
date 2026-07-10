namespace talentacquisition_jobplacement_mvc.Models
{
    public class CandidateProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public string? Summary { get; set; }
        public string? Experience { get; set; }
        public string? Education { get; set; }

        public ICollection<CV> CVs { get; set; } = new List<CV>();

        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}