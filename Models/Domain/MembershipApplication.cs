using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace ClubBAIST.Models.Domain
{
    public class MembershipApplication
    {
        [Key]
        public int ApplicationId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public string? AlternatePhone { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Occupation { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyPostalCode { get; set; }
        public string? CompanyPhone { get; set; }
        public int? Sponsor1MemberId { get; set; }
        public string? Sponsor1Signature { get; set; }
        public DateTime? Sponsor1Date { get; set; }
        public int? Sponsor2MemberId { get; set; }
        public string? Sponsor2Signature { get; set; }
        public DateTime? Sponsor2Date { get; set; }
        public int? DesiredTypeId { get; set; }
        public string Status { get; set; } = "Pending";
        public int? ReviewedByMemberId { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? Notes { get; set; }
        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        [ForeignKey("Sponsor1MemberId")]
        public Member? Sponsor1 { get; set; }

        [ForeignKey("Sponsor2MemberId")]
        public Member? Sponsor2 { get; set; }

        [ForeignKey("ReviewedByMemberId")]
        public Member? ReviewedBy { get; set; }

        [ForeignKey("DesiredTypeId")]
        public MembershipType? DesiredType { get; set; }
    }
}