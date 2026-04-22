using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubBAIST.Models.Domain
{
    public class TeeSheet
    {
        [Key]
        public int TeeSheetId { get; set; }
        public DateTime SheetDate { get; set; }
        public bool IsLocked { get; set; } = false;
        public string? Notes { get; set; }
        public int? CreatedByMemberId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CreatedByMemberId")]
        public Member? CreatedBy { get; set; }
        public ICollection<TeeTimeSlot> Slots { get; set; } = new List<TeeTimeSlot>();
    }
}