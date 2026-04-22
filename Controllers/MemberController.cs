using ClubBAIST.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubBAIST.Controllers
{
    public class MemberController : Controller
    {
        private readonly ClubBAISTContext _context;

        public MemberController(ClubBAISTContext context)
        {
            _context = context;
        }

        // GET: /Member/Profile?memberNumber=M001
        public IActionResult Profile(string? memberNumber)
        {
            if (string.IsNullOrEmpty(memberNumber))
                return RedirectToAction("Index", "Home");

            var member = _context.Members
                .Include(m => m.MembershipType)
                    .ThenInclude(t => t.Category)
                .Include(m => m.GolfRounds)
                .FirstOrDefault(m => m.MemberNumber == memberNumber
                                  && m.IsActive);

            if (member == null)
                return NotFound();

            ViewBag.ReservationCount = _context.TeeTimeReservations
                .Count(r => r.BookedByMemberId == member.MemberId
                         && r.Status == "Active");

            ViewBag.RoundCount = _context.GolfRounds
                .Count(r => r.MemberId == member.MemberId
                         && r.IsPosted);

            return View(member);
        }
    }
}