using System.ComponentModel.DataAnnotations;

namespace ClubBAIST.Models.ViewModels
{
    public class ScoreEntryViewModel
    {
        [Required(ErrorMessage = "Member number is required")]
        [Display(Name = "Member Number")]
        public string MemberNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a course")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Please select a tee")]
        public int TeeId { get; set; }

        [Required(ErrorMessage = "Date played is required")]
        [DataType(DataType.Date)]
        public DateTime PlayedDate { get; set; } = DateTime.Today;

        // 18 hole scores
        public List<HoleScoreEntry> HoleScores { get; set; } = new();

        // Populated for dropdowns
        public List<CourseOption> Courses { get; set; } = new();
        public List<TeeOption> Tees { get; set; } = new();

        // Calculated
        public int? FrontNine => HoleScores.Count >= 9
            ? HoleScores.Take(9).Sum(h => h.Score) : null;
        public int? BackNine => HoleScores.Count == 18
            ? HoleScores.Skip(9).Sum(h => h.Score) : null;
        public int? Total => HoleScores.Count == 18
            ? HoleScores.Sum(h => h.Score) : null;

        public string? ErrorMessage { get; set; }
    }

    public class HoleScoreEntry
    {
        public int HoleNumber { get; set; }
        public int Par { get; set; }
        public int? Yardage { get; set; }
        public int? StrokeIndex { get; set; }
        public int Score { get; set; }
    }

    public class CourseOption
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public bool IsClubCourse { get; set; }
    }

    public class TeeOption
    {
        public int TeeId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public decimal CourseRating { get; set; }
        public int SlopeRating { get; set; }
    }

    public class ScoreHistoryViewModel
    {
        public string MemberName { get; set; } = string.Empty;
        public decimal HandicapIndex { get; set; }
        public List<RoundSummary> Rounds { get; set; } = new();
        public decimal Last20Average { get; set; }
        public decimal Best8Average { get; set; }
    }

    public class RoundSummary
    {
        public int RoundId { get; set; }
        public DateTime PlayedDate { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string TeeName { get; set; } = string.Empty;
        public decimal CourseRating { get; set; }
        public int SlopeRating { get; set; }
        public int? TotalScore { get; set; }
        public decimal? ScoreDifferential { get; set; }
        public bool IsBest8 { get; set; }
    }
}