using ClubBAIST.Data;
using ClubBAIST.Models.Domain;
using ClubBAIST.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubBAIST.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly ClubBAISTContext _context;

        public ApplicationController(ClubBAISTContext context)
        {
            _context = context;
        }

        // =============================================
        // PUBLIC — Submit Application
        // =============================================

        // GET: /Application/Apply
        [HttpGet]
        public IActionResult Apply()
        {
            var model = new ApplicationViewModel
            {
                MembershipTypes = GetMembershipTypeOptions()
            };
            return View(model);
        }

        // POST: /Application/Apply
        [HttpPost]
        public IActionResult Apply(ApplicationViewModel model)
        {
            model.MembershipTypes = GetMembershipTypeOptions();

            if (!ModelState.IsValid)
                return View(model);

            // Validate Sponsor 1
            var sponsor1Error = ValidateSponsor(model.Sponsor1MemberNumber, "Sponsor 1");
            if (sponsor1Error != null)
            {
                model.ErrorMessage = sponsor1Error;
                return View(model);
            }

            // Validate Sponsor 2
            var sponsor2Error = ValidateSponsor(model.Sponsor2MemberNumber, "Sponsor 2");
            if (sponsor2Error != null)
            {
                model.ErrorMessage = sponsor2Error;
                return View(model);
            }

            // Make sure sponsors are different people
            if (model.Sponsor1MemberNumber == model.Sponsor2MemberNumber)
            {
                model.ErrorMessage = "Sponsor 1 and Sponsor 2 must be different members.";
                return View(model);
            }

            // Get sponsor member IDs
            var sponsor1 = _context.Members
                .First(m => m.MemberNumber == model.Sponsor1MemberNumber);
            var sponsor2 = _context.Members
                .First(m => m.MemberNumber == model.Sponsor2MemberNumber);

            // Check sponsor 1 hasn't exceeded 2 sponsorships this year
            var sponsor1Count = GetSponsorshipCountThisYear(sponsor1.MemberId);
            if (sponsor1Count >= 2)
            {
                model.ErrorMessage = $"Sponsor 1 ({sponsor1.FullName}) has already sponsored 2 members this year.";
                return View(model);
            }

            // Check sponsor 2 hasn't exceeded 2 sponsorships this year
            var sponsor2Count = GetSponsorshipCountThisYear(sponsor2.MemberId);
            if (sponsor2Count >= 2)
            {
                model.ErrorMessage = $"Sponsor 2 ({sponsor2.FullName}) has already sponsored 2 members this year.";
                return View(model);
            }

            // Create the application
            var application = new MembershipApplication
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                PostalCode = model.PostalCode,
                Phone = model.Phone,
                AlternatePhone = model.AlternatePhone,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth,
                Occupation = model.Occupation,
                CompanyName = model.CompanyName,
                CompanyAddress = model.CompanyAddress,
                CompanyPostalCode = model.CompanyPostalCode,
                CompanyPhone = model.CompanyPhone,
                DesiredTypeId = model.DesiredTypeId,
                Sponsor1MemberId = sponsor1.MemberId,
                Sponsor1Signature = model.Sponsor1Signature,
                Sponsor1Date = DateTime.Today,
                Sponsor2MemberId = sponsor2.MemberId,
                Sponsor2Signature = model.Sponsor2Signature,
                Sponsor2Date = DateTime.Today,
                Status = "Pending",
                SubmittedDate = DateTime.Now
            };

            _context.MembershipApplications.Add(application);
            _context.SaveChanges();

            TempData["AppSuccess"] = $"Application submitted successfully for {model.FirstName} {model.LastName}. Your application ID is #{application.ApplicationId}.";
            return RedirectToAction("ApplySuccess");
        }

        // GET: /Application/ApplySuccess
        public IActionResult ApplySuccess()
        {
            ViewBag.Message = TempData["AppSuccess"];
            return View();
        }

        // =============================================
        // PUBLIC — Check Application Status
        // =============================================

        // GET: /Application/Status
        [HttpGet]
        public IActionResult Status()
        {
            return View(new ApplicationStatusViewModel());
        }

        // POST: /Application/Status
        [HttpPost]
        public IActionResult Status(ApplicationStatusViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var application = _context.MembershipApplications
                .Include(a => a.DesiredType)
                .FirstOrDefault(a =>
                    a.LastName.ToLower() == model.LastName.ToLower() &&
                    a.Phone == model.Phone);

            if (application == null)
            {
                model.ErrorMessage = "No application found matching that last name and phone number.";
                return View(model);
            }

            model.ApplicantName = $"{application.FirstName} {application.LastName}";
            model.Status = application.Status;
            model.SubmittedDate = application.SubmittedDate;
            model.ReviewDate = application.ReviewDate;
            model.Notes = application.Notes;
            model.DesiredType = application.DesiredType?.TypeName;

            return View(model);
        }

        // =============================================
        // ADMIN — Review Applications
        // =============================================

        // GET: /Application/Review
        public IActionResult Review()
        {
            var applications = _context.MembershipApplications
                .Include(a => a.DesiredType)
                .Include(a => a.Sponsor1)
                .Include(a => a.Sponsor2)
                .OrderByDescending(a => a.SubmittedDate)
                .ToList();

            return View(applications);
        }

        // GET: /Application/ReviewDetail?id=1
        [HttpGet]
        public IActionResult ReviewDetail(int id)
        {
            var application = _context.MembershipApplications
                .Include(a => a.DesiredType)
                    .ThenInclude(t => t.Category)
                .Include(a => a.Sponsor1)
                .Include(a => a.Sponsor2)
                .FirstOrDefault(a => a.ApplicationId == id);

            if (application == null)
                return NotFound();

            var model = new ReviewApplicationViewModel
            {
                ApplicationId = application.ApplicationId,
                ApplicantName = $"{application.FirstName} {application.LastName}",
                Email = application.Email,
                Phone = application.Phone,
                DateOfBirth = application.DateOfBirth,
                Address = application.Address,
                Occupation = application.Occupation,
                DesiredTypeName = application.DesiredType?.TypeName ?? "Unknown",
                Sponsor1Name = application.Sponsor1?.FullName ?? "Unknown",
                Sponsor2Name = application.Sponsor2?.FullName ?? "Unknown",
                CurrentStatus = application.Status,
                SubmittedDate = application.SubmittedDate,
                Notes = application.Notes,
                NewStatus = application.Status
            };

            return View(model);
        }

        // POST: /Application/ReviewDetail
        [HttpPost]
        public IActionResult ReviewDetail(ReviewApplicationViewModel model)
        {
            // Validate reviewer
            var reviewer = _context.Members
                .FirstOrDefault(m => m.MemberNumber == model.ReviewerMemberNumber
                                  && m.IsActive);

            if (reviewer == null)
            {
                ModelState.AddModelError("ReviewerMemberNumber", "Reviewer member number not found.");
                return View(model);
            }

            var application = _context.MembershipApplications
                .Include(a => a.DesiredType)
                .FirstOrDefault(a => a.ApplicationId == model.ApplicationId);

            if (application == null)
                return NotFound();

            // Update application status
            application.Status = model.NewStatus;
            application.Notes = model.ReviewNotes;
            application.ReviewedByMemberId = reviewer.MemberId;
            application.ReviewDate = DateTime.Today;

            _context.SaveChanges();

            // If accepted, create member account
            if (model.NewStatus == "Accepted")
            {
                CreateMemberFromApplication(application);
            }

            TempData["Success"] = $"Application #{application.ApplicationId} updated to {model.NewStatus}.";
            return RedirectToAction("Review");
        }

        // =============================================
        // PRIVATE HELPERS
        // =============================================

        private string? ValidateSponsor(string memberNumber, string sponsorLabel)
        {
            var sponsor = _context.Members
                .Include(m => m.MembershipType)
                .FirstOrDefault(m => m.MemberNumber == memberNumber && m.IsActive);

            if (sponsor == null)
                return $"{sponsorLabel} member number '{memberNumber}' not found.";

            if (sponsor.MembershipType?.IsShareholderType == false)
                return $"{sponsorLabel} ({sponsor.FullName}) must be a Shareholder member.";

            if (!sponsor.IsGoodStanding)
                return $"{sponsorLabel} ({sponsor.FullName}) is not in good standing.";

            if (sponsor.YearsAsMember < 5)
                return $"{sponsorLabel} ({sponsor.FullName}) must have been a member for at least 5 years. " +
                       $"Current: {sponsor.YearsAsMember} year(s).";

            return null; // no error
        }

        private int GetSponsorshipCountThisYear(int sponsorMemberId)
        {
            var startOfYear = new DateTime(DateTime.Today.Year, 1, 1);
            return _context.MembershipApplications
                .Count(a => (a.Sponsor1MemberId == sponsorMemberId ||
                             a.Sponsor2MemberId == sponsorMemberId)
                          && a.SubmittedDate >= startOfYear
                          && a.Status != "Denied");
        }

        private void CreateMemberFromApplication(MembershipApplication application)
        {
            // Check if member already created
            var existing = _context.Members
                .FirstOrDefault(m => m.Email == application.Email
                                  && application.Email != null);
            if (existing != null) return;

            // Generate member number
            var lastMember = _context.Members
                .OrderByDescending(m => m.MemberId)
                .FirstOrDefault();

            int nextNum = 1;
            if (lastMember != null &&
                lastMember.MemberNumber.StartsWith("M") &&
                int.TryParse(lastMember.MemberNumber.Substring(1), out int lastNum))
            {
                nextNum = lastNum + 1;
            }

            var memberNumber = $"M{nextNum:D3}";
            var tempPassword = $"Welcome{nextNum:D3}";

            var newMember = new Member
            {
                MemberNumber = memberNumber,
                FirstName = application.FirstName,
                LastName = application.LastName,
                Address = application.Address,
                PostalCode = application.PostalCode,
                Phone = application.Phone,
                AlternatePhone = application.AlternatePhone,
                Email = application.Email,
                DateOfBirth = application.DateOfBirth,
                Occupation = application.Occupation,
                CompanyName = application.CompanyName,
                CompanyAddress = application.CompanyAddress,
                CompanyPostalCode = application.CompanyPostalCode,
                CompanyPhone = application.CompanyPhone,
                TypeId = application.DesiredTypeId ?? 1,
                MemberSince = DateTime.Today,
                IsGoodStanding = true,
                IsActive = true,
                Role = "Member",
                PasswordHash = tempPassword,
                CreatedAt = DateTime.Now
            };

            _context.Members.Add(newMember);
            _context.SaveChanges();

            // Create financial account
            var account = new MemberAccount
            {
                MemberId = newMember.MemberId,
                SharePurchasePaid = 0,
                EntranceFeePaid = 0,
                Balance = 0,
                CreatedAt = DateTime.Now
            };

            _context.MemberAccounts.Add(account);
            _context.SaveChanges();

            TempData["NewMember"] = $"Member account created. Member Number: {memberNumber}, Temp Password: {tempPassword}";
        }

        private List<MembershipTypeOption> GetMembershipTypeOptions()
        {
            return _context.MembershipTypes
                .Include(t => t.Category)
                .Where(t => t.HasGolfPrivileges)
                .Select(t => new MembershipTypeOption
                {
                    TypeId = t.TypeId,
                    DisplayName = $"{t.Category.CategoryName} — {t.TypeName} (${t.AnnualFee:N0}/yr)",
                    AnnualFee = t.AnnualFee
                })
                .ToList();
        }
    }
}