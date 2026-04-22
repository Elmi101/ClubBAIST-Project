using ClubBAIST.Data;
using ClubBAIST.Models.Domain;
using ClubBAIST.Models.ViewModels;
using ClubBAIST.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubBAIST.Controllers
{
    public class ScoreController : Controller
    {
        private readonly ClubBAISTContext _context;
        private readonly HandicapService _handicapService;

        public ScoreController(
            ClubBAISTContext context,
            HandicapService handicapService)
        {
            _context = context;
            _handicapService = handicapService;
        }

        // GET: /Score/Enter
        [HttpGet]
        public IActionResult Enter()
        {
            var model = new ScoreEntryViewModel
            {
                PlayedDate = DateTime.Today,
                Courses = GetCourseOptions(),
                Tees = new List<TeeOption>(),
                HoleScores = new List<HoleScoreEntry>()
            };
            return View(model);
        }

        // GET: /Score/GetTees?courseId=1
        // AJAX endpoint to load tees when course is selected
        public IActionResult GetTees(int courseId)
        {
            var tees = _context.CourseTees
                .Where(t => t.CourseId == courseId)
                .Select(t => new
                {
                    teeId = t.TeeId,
                    displayName = $"{t.TeeName} — {t.Gender} " +
                                  $"(Rating: {t.CourseRating} / Slope: {t.SlopeRating})",
                    courseRating = t.CourseRating,
                    slopeRating = t.SlopeRating
                })
                .ToList();

            return Json(tees);
        }

        // GET: /Score/GetHoles?teeId=1
        // AJAX endpoint to load hole data when tee is selected
        public IActionResult GetHoles(int teeId)
        {
            var holes = _context.CourseHoles
                .Where(h => h.TeeId == teeId)
                .OrderBy(h => h.HoleNumber)
                .Select(h => new
                {
                    holeNumber = h.HoleNumber,
                    par = h.Par,
                    yardage = h.Yardage,
                    strokeIndex = h.StrokeIndex
                })
                .ToList();

            return Json(holes);
        }

        // POST: /Score/Enter
        [HttpPost]
        public IActionResult Enter(
            string MemberNumber,
            int CourseId,
            int TeeId,
            DateTime PlayedDate,
            List<int> Scores)
        {
            var model = new ScoreEntryViewModel
            {
                MemberNumber = MemberNumber,
                CourseId = CourseId,
                TeeId = TeeId,
                PlayedDate = PlayedDate,
                Courses = GetCourseOptions(),
                Tees = new List<TeeOption>()
            };

            // Validate member
            var member = _context.Members
                .Include(m => m.MembershipType)
                .FirstOrDefault(m => m.MemberNumber == MemberNumber
                                  && m.IsActive);

            if (member == null)
            {
                model.ErrorMessage = "Member number not found.";
                return View(model);
            }

            if (member.MembershipType?.HasGolfPrivileges == false)
            {
                model.ErrorMessage = "This member type does not have golf privileges.";
                return View(model);
            }

            // Validate scores
            if (Scores == null || Scores.Count != 18)
            {
                model.ErrorMessage = "Please enter scores for all 18 holes.";
                return View(model);
            }

            if (Scores.Any(s => s < 1 || s > 20))
            {
                model.ErrorMessage = "All scores must be between 1 and 20.";
                return View(model);
            }

            // Validate date not in future
            if (PlayedDate > DateTime.Today)
            {
                model.ErrorMessage = "Played date cannot be in the future.";
                return View(model);
            }

            // Get tee info
            var tee = _context.CourseTees
                .FirstOrDefault(t => t.TeeId == TeeId);

            if (tee == null)
            {
                model.ErrorMessage = "Invalid tee selection.";
                return View(model);
            }

            int totalScore = Scores.Sum();

            // Create the round
            var round = new GolfRound
            {
                MemberId = member.MemberId,
                CourseId = CourseId,
                TeeId = TeeId,
                PlayedDate = PlayedDate,
                TotalScore = totalScore,
                IsPosted = false,
                EnteredAt = DateTime.Now
            };

            _context.GolfRounds.Add(round);
            _context.SaveChanges();

            // Save hole scores
            for (int i = 0; i < 18; i++)
            {
                _context.RoundHoleScores.Add(new RoundHoleScore
                {
                    RoundId = round.RoundId,
                    HoleNumber = i + 1,
                    Score = Scores[i]
                });
            }

            _context.SaveChanges();

            // Post the round and calculate differential
            _handicapService.PostRound(round);

            TempData["Success"] = $"Round posted for {member.FullName}. " +
                                  $"Score: {totalScore}. " +
                                  $"Differential: {round.ScoreDifferential:F1}";

            return RedirectToAction("History",
                new { memberNumber = MemberNumber });
        }

        // GET: /Score/History?memberNumber=M001
        public IActionResult History(string memberNumber)
        {
            var member = _context.Members
                .FirstOrDefault(m => m.MemberNumber == memberNumber);

            if (member == null)
                return NotFound();

            var rounds = _handicapService.GetLast20Rounds(member.MemberId);
            var differentials = rounds
                .Where(r => r.ScoreDifferential.HasValue)
                .Select(r => r.ScoreDifferential!.Value)
                .ToList();

            // Identify best 8
            var best8Values = differentials
                .OrderBy(d => d)
                .Take(8)
                .ToList();

            decimal handicapIndex = _handicapService
                .CalculateHandicapIndex(differentials);

            decimal last20Avg = differentials.Any()
                ? Math.Round(differentials.Average(), 1) : 0;

            decimal best8Avg = best8Values.Any()
                ? Math.Round(best8Values.Average(), 1) : 0;

            var viewModel = new ScoreHistoryViewModel
            {
                MemberName = member.FullName,
                HandicapIndex = handicapIndex,
                Last20Average = last20Avg,
                Best8Average = best8Avg,
                Rounds = rounds.Select(r => new RoundSummary
                {
                    RoundId = r.RoundId,
                    PlayedDate = r.PlayedDate,
                    CourseName = r.Course?.CourseName ?? "Unknown",
                    TeeName = r.Tee?.TeeName ?? "Unknown",
                    CourseRating = r.Tee?.CourseRating ?? 0,
                    SlopeRating = r.Tee?.SlopeRating ?? 0,
                    TotalScore = r.TotalScore,
                    ScoreDifferential = r.ScoreDifferential,
                    IsBest8 = r.ScoreDifferential.HasValue &&
                              best8Values.Contains(r.ScoreDifferential.Value)
                }).ToList()
            };

            ViewBag.MemberNumber = memberNumber;
            return View(viewModel);
        }

        // GET: /Score/Detail?roundId=1
        public IActionResult Detail(int roundId)
        {
            var round = _context.GolfRounds
                .Include(r => r.Member)
                .Include(r => r.Course)
                .Include(r => r.Tee)
                .Include(r => r.HoleScores)
                .FirstOrDefault(r => r.RoundId == roundId);

            if (round == null)
                return NotFound();

            var holes = _context.CourseHoles
                .Where(h => h.TeeId == round.TeeId)
                .OrderBy(h => h.HoleNumber)
                .ToList();

            ViewBag.Holes = holes;
            return View(round);
        }

        private List<CourseOption> GetCourseOptions()
        {
            return _context.GolfCourses
                .Where(c => c.IsGolfCanadaApproved)
                .Select(c => new CourseOption
                {
                    CourseId = c.CourseId,
                    CourseName = c.CourseName,
                    IsClubCourse = c.IsClubCourse
                })
                .ToList();
        }
    }
}