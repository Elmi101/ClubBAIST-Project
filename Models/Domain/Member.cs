using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubBAIST.Models.Domain
{
    public class Member
    {
        [Key]
        public int MemberId { get; set; }
        public string MemberNumber { get; set; } = string.Empty;
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
        public int TypeId { get; set; }
        public DateTime MemberSince { get; set; }
        public bool IsGoodStanding { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string? PasswordHash { get; set; }
        public string Role { get; set; } = "Member";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("TypeId")]
        public MembershipType MembershipType { get; set; } = null!;

        public MemberAccount? Account { get; set; }
        public ICollection<TeeTimeReservation> Reservations { get; set; } = new List<TeeTimeReservation>();
        public ICollection<GolfRound> GolfRounds { get; set; } = new List<GolfRound>();

        public string FullName => $"{FirstName} {LastName}";
        public int YearsAsMember => (int)((DateTime.Today - MemberSince).TotalDays / 365.25);
    }
}