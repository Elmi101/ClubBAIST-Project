using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubBAIST.Models.Domain
{
    public class TeeTimeReservation
    {
        [Key]
        public int ReservationId { get; set; }
        public int SlotId { get; set; }
        public int BookedByMemberId { get; set; }
        public int NumberOfPlayers { get; set; }
        public int? NumberOfCarts { get; set; }
        public string Status { get; set; } = "Active";
        public bool IsStandingRequest { get; set; } = false;
        public DateTime BookedAt { get; set; } = DateTime.Now;
        public DateTime? CancelledAt { get; set; }
        public int? CancelledByMemberId { get; set; }

        [ForeignKey("SlotId")]
        public TeeTimeSlot Slot { get; set; } = null!;

        [ForeignKey("BookedByMemberId")]
        public Member BookedBy { get; set; } = null!;

        [ForeignKey("CancelledByMemberId")]
        public Member? CancelledBy { get; set; }

        public ICollection<ReservationPlayer> Players { get; set; } = new List<ReservationPlayer>();
    }
}