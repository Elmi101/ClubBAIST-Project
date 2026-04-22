using System.ComponentModel.DataAnnotations;

namespace ClubBAIST.Models.Domain
{
    public class MembershipCategory
    {
        [Key]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public ICollection<MembershipType> MembershipTypes { get; set; } = new List<MembershipType>();
    }
}