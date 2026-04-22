using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubBAIST.Models.Domain
{
    public class RoundHoleScore
    {
        [Key]
        public int HoleScoreId { get; set; }
        public int RoundId { get; set; }
        public int HoleNumber { get; set; }
        public int Score { get; set; }

        [ForeignKey("RoundId")]
        public GolfRound Round { get; set; } = null!;
    }
}