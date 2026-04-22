using ClubBAIST.Data;
using ClubBAIST.Models.Domain;
using ClubBAIST.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubBAIST.Controllers
{
    public class StandingRequestController : Controller
    {
        private readonly ClubBAISTContext _context;

        public StandingRequestController(ClubBAISTContext context)
        {
            _context = context;
        }

        // GET: /StandingRequest/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new StandingRequestViewModel
            {
                RequestedStartDate = DateTime.Today
            });
        }

        // POST: /StandingRequest/Create
        [HttpPost]
        public IActionResult Create(StandingRequestViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Validate shareholder member
            var shareholder = _context.Members
                .Include(m => m.MembershipType)
                .FirstOrDefault(m => m.MemberNumber == model.ShareholderMemberNumber
                                  && m.IsActive);

            if (shareholder == null)
            {
                model.ErrorMessage = "Shareholder member number not found.";
                return View(model);
            }

            if (shareholder.MembershipType?.IsShareholderType == false)
            {
                model.ErrorMessage = "Only Shareholder members can make standing tee time requests.";
                return View(model);
            }

            if (!shareholder.IsGoodStanding)
            {
                model.ErrorMessage = "Member is not in good standing.";
                return View(model);
            }

            // Check for existing active standing request
            var existingRequest = _context.StandingTeeTimeRequests
                .FirstOrDefault(r => r.ShareholderMemberId == shareholder.MemberId
                                  && r.IsActive);

            if (existingRequest != null)
            {
                model.ErrorMessage = "This shareholder already has an active standing tee time request. Cancel it first.";
                return View(model);
            }

            // Resolve optional member IDs
            int? member2Id = ResolveMemberId(model.Member2Number);
            int? member3Id = ResolveMemberId(model.Member3Number);
            int? member4Id = ResolveMemberId(model.Member4Number);

            var request = new StandingTeeTimeRequest
            {
                ShareholderMemberId = shareholder.MemberId,
                Member2Id = member2Id,
                Member3Id = member3Id,
                Member4Id = member4Id,
                RequestedDayOfWeek = model.RequestedDayOfWeek,
                RequestedTime = model.RequestedTime,
                RequestedStartDate = model.RequestedStartDate,
                RequestedEndDate = model.RequestedEndDate,
                IsActive = true
            };

            _context.StandingTeeTimeRequests.Add(request);
            _context.SaveChanges();

            TempData["Success"] = "Standing tee time request submitted successfully.";
            return RedirectToAction("MyRequests",
                new { memberNumber = model.ShareholderMemberNumber });
        }

        // GET: /StandingRequest/MyRequests?memberNumber=M001
        public IActionResult MyRequests(string memberNumber)
        {
            var member = _context.Members
                .FirstOrDefault(m => m.MemberNumber == memberNumber);

            if (member == null)
                return NotFound();

            var requests = _context.StandingTeeTimeRequests
                .Where(r => r.ShareholderMemberId == member.MemberId)
                .OrderByDescending(r => r.RequestedStartDate)
                .ToList();

            ViewBag.MemberName = member.FullName;
            ViewBag.MemberNumber = memberNumber;
            return View(requests);
        }

        // POST: /StandingRequest/Cancel
        [HttpPost]
        public IActionResult Cancel(int standingRequestId, string memberNumber)
        {
            var request = _context.StandingTeeTimeRequests
                .FirstOrDefault(r => r.StandingRequestId == standingRequestId);

            if (request == null)
                return NotFound();

            request.IsActive = false;
            _context.SaveChanges();

            TempData["Success"] = "Standing tee time request cancelled.";
            return RedirectToAction("MyRequests", new { memberNumber });
        }

        private int? ResolveMemberId(string? memberNumber)
        {
            if (string.IsNullOrWhiteSpace(memberNumber)) return null;
            return _context.Members
                .FirstOrDefault(m => m.MemberNumber == memberNumber && m.IsActive)
                ?.MemberId;
        }
    }
}