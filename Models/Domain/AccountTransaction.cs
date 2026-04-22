using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubBAIST.Models.Domain
{
    public class AccountTransaction
    {
        [Key]
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Today;
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public int? PostedByMemberId { get; set; }

        [ForeignKey("AccountId")]
        public MemberAccount Account { get; set; } = null!;

        [ForeignKey("PostedByMemberId")]
        public Member? PostedBy { get; set; }
    }
}