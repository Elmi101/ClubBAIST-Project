using ClubBAIST.Data;
using ClubBAIST.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClubBAIST.Controllers
{
    public class HandicapController : Controller
    {
        private readonly ClubBAISTContext _context;
        private readonly HandicapService _handicapService;

        public HandicapController(
            ClubBAISTContext context,
            HandicapService handicapService)
        {
            _context = context;
            _handicapService = handicapService;
        }

        // GET: /Handicap/Index?memberNumber=M001
        public IActionResult Index(string? memberNumber)
        {
            if (string.IsNullOrEmpty(memberNumber))
                return View("SelectMember");

            var member = _context.Members
                .FirstOrDefault(m => m.MemberNumber == memberNumber
                                  && m.IsActive);

            if (member == null)
            {
                ViewBag.Error = $"Member '{memberNumber}' not found.";
                return View("SelectMember");
            }

            var handicapIndex = _handicapService
                .GetHandicapIndex(member.MemberId);

            var rounds = _handicapService
                .GetLast20Rounds(member.MemberId);

            var differentials = rounds
                .Where(r => r.ScoreDifferential.HasValue)
                .Select(r => r.ScoreDifferential!.Value)
                .OrderBy(d => d)
                .ToList();

            ViewBag.MemberName = member.FullName;
            ViewBag.MemberNumber = memberNumber;
            ViewBag.HandicapIndex = handicapIndex;
            ViewBag.RoundCount = rounds.Count;
            ViewBag.Differentials = differentials;

            return View(rounds);
        }
    }
}