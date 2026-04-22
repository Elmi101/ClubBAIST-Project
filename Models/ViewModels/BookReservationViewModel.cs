using System.ComponentModel.DataAnnotations;

namespace ClubBAIST.Models.ViewModels
{
    public class BookReservationViewModel
    {
        public int SlotId { get; set; }
        public string SlotTime { get; set; } = string.Empty;
        public DateTime SheetDate { get; set; }

        [Required]
        [Display(Name = "Number of Players")]
        [Range(1, 4, ErrorMessage = "Must be between 1 and 4 players")]
        public int NumberOfPlayers { get; set; } = 1;

        [Display(Name = "Number of Carts")]
        [Range(0, 2)]
        public int NumberOfCarts { get; set; } = 0;

        [Required]
        [Display(Name = "Booked By Member Number")]
        public string BookedByMemberNumber { get; set; } = string.Empty;

        // Additional players (up to 3 more)
        [Display(Name = "Player 2 Member Number (optional)")]
        public string? Player2MemberNumber { get; set; }

        [Display(Name = "Player 3 Member Number (optional)")]
        public string? Player3MemberNumber { get; set; }

        [Display(Name = "Player 4 Member Number (optional)")]
        public string? Player4MemberNumber { get; set; }

        [Display(Name = "Player 2 Guest Name (if not a member)")]
        public string? Player2GuestName { get; set; }

        [Display(Name = "Player 3 Guest Name (if not a member)")]
        public string? Player3GuestName { get; set; }

        [Display(Name = "Player 4 Guest Name (if not a member)")]
        public string? Player4GuestName { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}