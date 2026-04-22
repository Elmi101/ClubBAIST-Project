using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubBAIST.Models.Domain
{
    public class CourseHole
    {
        [Key]
        public int HoleId { get; set; }
        public int TeeId { get; set; }
        public int HoleNumber { get; set; }
        public int Par { get; set; }
        public int? Yardage { get; set; }
        public int? StrokeIndex { get; set; }

        [ForeignKey("TeeId")]
        public CourseTee Tee { get; set; } = null!;
    }
}