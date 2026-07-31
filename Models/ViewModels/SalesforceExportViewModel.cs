using System.ComponentModel.DataAnnotations;

namespace talentacquisition_jobplacement_mvc.Models.ViewModels
{
    public class SalesforceExportViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Company")]
        public string? Company { get; set; }

        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [Display(Name = "Industry")]
        public string? Industry { get; set; }

        [Display(Name = "Job Title")]
        public string? Title { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}