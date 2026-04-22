using System.ComponentModel.DataAnnotations;

namespace ClubBAIST.Models.Domain
{
    public class GolfCourse
    {
        [Key]
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public bool IsClubCourse { get; set; } = false;
        public bool IsGolfCanadaApproved { get; set; } = true;
        public string? Address { get; set; }
        public string? City { get; set; }

        public ICollection<CourseTee> Tees { get; set; } = new List<CourseTee>();
    }
}