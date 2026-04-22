using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubBAIST.Models.Domain
{
    public class TeeTimeSlot
    {
        [Key]
        public int SlotId { get; set; }
        public int TeeSheetId { get; set; }
        public TimeSpan SlotTime { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsBlocked { get; set; } = false;
        public string? BlockReason { get; set; }

        [ForeignKey("TeeSheetId")]
        public TeeSheet TeeSheet { get; set; } = null!;
        public TeeTimeReservation? Reservation { get; set; }
    }
}