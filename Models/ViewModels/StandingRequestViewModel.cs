using System.ComponentModel.DataAnnotations;

namespace ClubBAIST.Models.ViewModels
{
    public class StandingRequestViewModel
    {
        public int? StandingRequestId { get; set; }

        [Required]
        [Display(Name = "Shareholder Member Number")]
        public string ShareholderMemberNumber { get; set; } = string.Empty;

        [Display(Name = "Member 2 Number")]
        public string? Member2Number { get; set; }

        [Display(Name = "Member 3 Number")]
        public string? Member3Number { get; set; }

        [Display(Name = "Member 4 Number")]
        public string? Member4Number { get; set; }

        [Required]
        [Display(Name = "Requested Day of Week")]
        public int RequestedDayOfWeek { get; set; }

        [Required]
        [Display(Name = "Requested Tee Time")]
        public TimeSpan RequestedTime { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime RequestedStartDate { get; set; } = DateTime.Today;

        [Display(Name = "End Date")]
        public DateTime? RequestedEndDate { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}