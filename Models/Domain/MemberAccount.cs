using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubBAIST.Models.Domain
{
    public class MemberAccount
    {
        [Key]
        public int AccountId { get; set; }
        public int MemberId { get; set; }
        public decimal SharePurchasePaid { get; set; } = 0;
        public decimal EntranceFeePaid { get; set; } = 0;
        public decimal Balance { get; set; } = 0;
        public DateTime? LastStatementDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("MemberId")]
        public Member Member { get; set; } = null!;
        public ICollection<AccountTransaction> Transactions { get; set; } = new List<AccountTransaction>();
    }
}