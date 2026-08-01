using System.ComponentModel.DataAnnotations;

namespace talentacquisition_jobplacement_mvc.Models.ViewModels
{
    public class SupportTicketViewModel
    {
        [Required]
        [Display(Name = "Summary")]
        public string Summary { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Average"; // High, Average, Low
    }
}