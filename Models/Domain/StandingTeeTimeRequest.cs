using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubBAIST.Models.Domain
{
    public class StandingTeeTimeRequest
    {
        [Key]
        public int StandingRequestId { get; set; }
        public int ShareholderMemberId { get; set; }
        public int? Member2Id { get; set; }
        public int? Member3Id { get; set; }
        public int? Member4Id { get; set; }
        public int RequestedDayOfWeek { get; set; }
        public TimeSpan RequestedTime { get; set; }
        public DateTime RequestedStartDate { get; set; }
        public DateTime? RequestedEndDate { get; set; }
        public int? PriorityNumber { get; set; }
        public TimeSpan? ApprovedTime { get; set; }
        public int? ApprovedByMemberId { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey("ShareholderMemberId")]
        public Member ShareholderMember { get; set; } = null!;

        [ForeignKey("Member2Id")]
        public Member? Member2 { get; set; }

        [ForeignKey("Member3Id")]
        public Member? Member3 { get; set; }

        [ForeignKey("Member4Id")]
        public Member? Member4 { get; set; }

        [ForeignKey("ApprovedByMemberId")]
        public Member? ApprovedBy { get; set; }
    }
}