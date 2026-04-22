using System.ComponentModel.DataAnnotations;

namespace ClubBAIST.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Member number is required")]
        [Display(Name = "Member Number")]
        public string MemberNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }
}