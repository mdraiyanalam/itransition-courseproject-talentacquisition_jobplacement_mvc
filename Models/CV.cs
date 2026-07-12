namespace talentacquisition_jobplacement_mvc.Models
{
    public class CV
    {
        public int Id { get; set; }

        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string AttributeValues { get; set; } = "{}";
        public bool IsPublished { get; set; } = true;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<CVLike> Likes { get; set; } = new List<CVLike>();
    }
}