using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubBAIST.Models.Domain
{
    public class GolfRound
    {
        [Key]
        public int RoundId { get; set; }
        public int MemberId { get; set; }
        public int CourseId { get; set; }
        public int TeeId { get; set; }
        public DateTime PlayedDate { get; set; }
        public int? TotalScore { get; set; }
        public decimal? ScoreDifferential { get; set; }
        public bool IsPosted { get; set; } = false;
        public DateTime? PostedAt { get; set; }
        public DateTime EnteredAt { get; set; } = DateTime.Now;

        [ForeignKey("MemberId")]
        public Member Member { get; set; } = null!;

        [ForeignKey("CourseId")]
        public GolfCourse Course { get; set; } = null!;

        [ForeignKey("TeeId")]
        public CourseTee Tee { get; set; } = null!;

        public ICollection<RoundHoleScore> HoleScores { get; set; } = new List<RoundHoleScore>();
    }
}