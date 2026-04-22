using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubBAIST.Models.Domain
{
    public class MembershipType
    {
        [Key]
        public int TypeId { get; set; }
        public int CategoryId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public decimal AnnualFee { get; set; }
        public bool IsShareholderType { get; set; }
        public bool HasGolfPrivileges { get; set; } = true;
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? TeeTimeRestriction { get; set; }

        [ForeignKey("CategoryId")]
        public MembershipCategory Category { get; set; } = null!;
        public ICollection<Member> Members { get; set; } = new List<Member>();
    }
}