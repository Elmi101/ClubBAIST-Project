using ClubBAIST.Data;
using ClubBAIST.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ClubBAIST.Services
{
    public class HandicapService
    {
        private readonly ClubBAISTContext _context;

        public HandicapService(ClubBAISTContext context)
        {
            _context = context;
        }

        // WHS Score Differential formula
        // (Adjusted Gross Score - Course Rating) x 113 / Slope Rating
        public decimal CalculateScoreDifferential(
            int adjustedGrossScore,
            decimal courseRating,
            int slopeRating)
        {
            return Math.Round(
                (adjustedGrossScore - courseRating) * 113m / slopeRating, 1);
        }

        // WHS Handicap Index = average of best 8 of last 20 differentials
        public decimal CalculateHandicapIndex(List<decimal> differentials)
        {
            if (!differentials.Any()) return 0;

            // Take last 20
            var last20 = differentials
                .OrderByDescending(d => d)
                .TakeLast(20)
                .ToList();

            // Best 8 (lowest differentials)
            var best8 = last20.OrderBy(d => d).Take(8).ToList();

            return Math.Round(best8.Average(), 1);
        }

        // Get last 20 rounds with differentials for a member
        public List<GolfRound> GetLast20Rounds(int memberId)
        {
            return _context.GolfRounds
                .Include(r => r.Course)
                .Include(r => r.Tee)
                .Include(r => r.HoleScores)
                .Where(r => r.MemberId == memberId && r.IsPosted)
                .OrderByDescending(r => r.PlayedDate)
                .Take(20)
                .ToList();
        }

        // Calculate and save differential when a round is posted
        public void PostRound(GolfRound round)
        {
            var tee = _context.CourseTees
                .FirstOrDefault(t => t.TeeId == round.TeeId);

            if (tee == null) return;

            round.ScoreDifferential = CalculateScoreDifferential(
                round.TotalScore ?? 0,
                tee.CourseRating,
                tee.SlopeRating);

            round.IsPosted = true;
            round.PostedAt = DateTime.Now;

            _context.SaveChanges();
        }

        // Get current handicap index for a member
        public decimal GetHandicapIndex(int memberId)
        {
            var rounds = GetLast20Rounds(memberId);
            var differentials = rounds
                .Where(r => r.ScoreDifferential.HasValue)
                .Select(r => r.ScoreDifferential!.Value)
                .ToList();

            return CalculateHandicapIndex(differentials);
        }

        // WHS: Apply max score per hole (net double bogey)
        // Max score = par + 2 + any handicap strokes received
        public int AdjustedGrossScore(
            List<int> holeScores,
            List<CourseHole> holes,
            decimal handicapIndex)
        {
            int total = 0;
            int courseHandicap = (int)Math.Round(handicapIndex);

            for (int i = 0; i < holeScores.Count && i < holes.Count; i++)
            {
                var hole = holes[i];
                var score = holeScores[i];

                // Extra strokes for this hole based on stroke index
                int extraStrokes = courseHandicap >= hole.StrokeIndex ? 1 : 0;
                int maxScore = hole.Par + 2 + extraStrokes;

                // Cap at max score
                total += Math.Min(score, maxScore);
            }

            return total;
        }
    }
}