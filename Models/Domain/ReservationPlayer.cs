using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubBAIST.Models.Domain
{
    public class ReservationPlayer
    {
        [Key]
        public int ReservationPlayerId { get; set; }
        public int ReservationId { get; set; }
        public int? MemberId { get; set; }
        public string? GuestName { get; set; }
        public bool HasCheckedIn { get; set; } = false;

        [ForeignKey("ReservationId")]
        public TeeTimeReservation Reservation { get; set; } = null!;

        [ForeignKey("MemberId")]
        public Member? Member { get; set; }
    }
}