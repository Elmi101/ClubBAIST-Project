using ClubBAIST.Data;
using ClubBAIST.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubBAIST.Controllers
{
    public class ClerkController : Controller
    {
        private readonly ClubBAISTContext _context;

        public ClerkController(ClubBAISTContext context)
        {
            _context = context;
        }

        // GET: /Clerk/Members
        public IActionResult Members()
        {
            var members = _context.Members
                .Include(m => m.MembershipType)
                    .ThenInclude(t => t.Category)
                .Where(m => m.IsActive)
                .OrderBy(m => m.LastName)
                .ToList();

            return View(members);
        }

        // GET: /Clerk/CreateTeeSheet
        [HttpGet]
        public IActionResult CreateTeeSheet()
        {
            ViewBag.ExistingSheets = _context.TeeSheets
                .OrderByDescending(ts => ts.SheetDate)
                .Take(14)
                .ToList();

            return View();
        }

        // POST: /Clerk/CreateTeeSheet
        [HttpPost]
        public IActionResult CreateTeeSheet(
            DateTime sheetDate,
            string startTime,
            string endTime,
            string? notes,
            string clerkMemberNumber)
        {
            // Validate clerk
            var clerk = _context.Members
                .FirstOrDefault(m => m.MemberNumber == clerkMemberNumber
                                  && m.IsActive);

            if (clerk == null)
            {
                TempData["Error"] = "Clerk member number not found.";
                return RedirectToAction("CreateTeeSheet");
            }

            // Check if sheet already exists
            if (_context.TeeSheets.Any(ts => ts.SheetDate == sheetDate.Date))
            {
                TempData["Error"] = $"A tee sheet already exists for " +
                    $"{sheetDate:MMMM dd, yyyy}.";
                return RedirectToAction("CreateTeeSheet");
            }

            // Create the tee sheet
            var teeSheet = new TeeSheet
            {
                SheetDate = sheetDate.Date,
                IsLocked = false,
                Notes = notes,
                CreatedByMemberId = clerk.MemberId,
                CreatedAt = DateTime.Now
            };

            _context.TeeSheets.Add(teeSheet);
            _context.SaveChanges();

            // Generate 8-minute slots
            var start = TimeSpan.Parse(startTime);
            var end = TimeSpan.Parse(endTime);
            var current = start;
            int slotCount = 0;

            while (current <= end)
            {
                _context.TeeTimeSlots.Add(new TeeTimeSlot
                {
                    TeeSheetId = teeSheet.TeeSheetId,
                    SlotTime = current,
                    IsAvailable = true,
                    IsBlocked = false
                });

                current = current.Add(TimeSpan.FromMinutes(8));
                slotCount++;
            }

            _context.SaveChanges();

            TempData["Success"] = $"Tee sheet created for " +
                $"{sheetDate:MMMM dd, yyyy} with {slotCount} slots.";
            return RedirectToAction("CreateTeeSheet");
        }

        // POST: /Clerk/LockTeeSheet
        [HttpPost]
        public IActionResult LockTeeSheet(int teeSheetId)
        {
            var sheet = _context.TeeSheets
                .FirstOrDefault(ts => ts.TeeSheetId == teeSheetId);

            if (sheet != null)
            {
                sheet.IsLocked = !sheet.IsLocked;
                _context.SaveChanges();
                TempData["Success"] = sheet.IsLocked
                    ? "Tee sheet locked."
                    : "Tee sheet unlocked.";
            }

            return RedirectToAction("CreateTeeSheet");
        }

        // POST: /Clerk/BlockSlot
        [HttpPost]
        public IActionResult BlockSlot(
            int slotId,
            string blockReason,
            string sheetDate)
        {
            var slot = _context.TeeTimeSlots
                .FirstOrDefault(s => s.SlotId == slotId);

            if (slot != null)
            {
                slot.IsBlocked = true;
                slot.IsAvailable = false;
                slot.BlockReason = blockReason;
                _context.SaveChanges();
                TempData["Success"] = "Slot blocked successfully.";
            }

            return RedirectToAction("ViewSheet", "TeeSheet",
                new { date = sheetDate });
        }

        // GET: /Clerk/ManageStandingRequests
        public IActionResult ManageStandingRequests()
        {
            var requests = _context.StandingTeeTimeRequests
                .Include(r => r.ShareholderMember)
                .Include(r => r.Member2)
                .Include(r => r.Member3)
                .Include(r => r.Member4)
                .Include(r => r.ApprovedBy)
                .Where(r => r.IsActive)
                .OrderBy(r => r.PriorityNumber)
                .ToList();

            return View(requests);
        }

        // POST: /Clerk/ApproveStandingRequest
        [HttpPost]
        public IActionResult ApproveStandingRequest(
            int standingRequestId,
            int priorityNumber,
            string approvedTime,
            string clerkMemberNumber)
        {
            var request = _context.StandingTeeTimeRequests
                .FirstOrDefault(r => r.StandingRequestId == standingRequestId);

            var clerk = _context.Members
                .FirstOrDefault(m => m.MemberNumber == clerkMemberNumber);

            if (request != null && clerk != null)
            {
                request.PriorityNumber = priorityNumber;
                request.ApprovedTime = TimeSpan.Parse(approvedTime);
                request.ApprovedByMemberId = clerk.MemberId;
                request.ApprovedDate = DateTime.Today;
                _context.SaveChanges();

                TempData["Success"] =
                    $"Standing request approved with priority #{priorityNumber}.";
            }

            return RedirectToAction("ManageStandingRequests");
        }
    }
}