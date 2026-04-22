using System.ComponentModel.DataAnnotations;

namespace ClubBAIST.Models.ViewModels
{
    public class ApplicationViewModel
    {
        // Personal Info
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Alternate Phone")]
        public string? AlternatePhone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        public string? Occupation { get; set; }

        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [Display(Name = "Company Address")]
        public string? CompanyAddress { get; set; }

        [Display(Name = "Company Postal Code")]
        public string? CompanyPostalCode { get; set; }

        [Display(Name = "Company Phone")]
        public string? CompanyPhone { get; set; }

        // Membership Type
        [Required(ErrorMessage = "Please select a membership type")]
        [Display(Name = "Desired Membership Type")]
        public int DesiredTypeId { get; set; }

        // Sponsors
        [Required(ErrorMessage = "Sponsor 1 member number is required")]
        [Display(Name = "Sponsor 1 Member Number")]
        public string Sponsor1MemberNumber { get; set; } = string.Empty;

        [Display(Name = "Sponsor 1 Signature")]
        public string? Sponsor1Signature { get; set; }

        [Required(ErrorMessage = "Sponsor 2 member number is required")]
        [Display(Name = "Sponsor 2 Member Number")]
        public string Sponsor2MemberNumber { get; set; } = string.Empty;

        [Display(Name = "Sponsor 2 Signature")]
        public string? Sponsor2Signature { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        // Populated from DB for the dropdown
        public List<MembershipTypeOption> MembershipTypes { get; set; } = new();
    }

    public class MembershipTypeOption
    {
        public int TypeId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public decimal AnnualFee { get; set; }
    }

    public class ApplicationStatusViewModel
    {
        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        // Result
        public string? ApplicantName { get; set; }
        public string? Status { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? Notes { get; set; }
        public string? DesiredType { get; set; }
    }

    public class ReviewApplicationViewModel
    {
        public int ApplicationId { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public string DesiredTypeName { get; set; } = string.Empty;
        public string Sponsor1Name { get; set; } = string.Empty;
        public string Sponsor2Name { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public string? Notes { get; set; }

        // For the review form
        [Required]
        [Display(Name = "Decision")]
        public string NewStatus { get; set; } = string.Empty;

        [Display(Name = "Review Notes")]
        public string? ReviewNotes { get; set; }

        [Required(ErrorMessage = "Reviewer member number is required")]
        [Display(Name = "Your Member Number")]
        public string ReviewerMemberNumber { get; set; } = string.Empty;
    }
}