using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubBAIST.Models.Domain
{
    public class CourseTee
    {
        [Key]
        public int TeeId { get; set; }
        public int CourseId { get; set; }
        public string TeeName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public decimal CourseRating { get; set; }
        public int SlopeRating { get; set; }
        public int TotalPar { get; set; }

        [ForeignKey("CourseId")]
        public GolfCourse Course { get; set; } = null!;
        public ICollection<CourseHole> Holes { get; set; } = new List<CourseHole>();
    }
}