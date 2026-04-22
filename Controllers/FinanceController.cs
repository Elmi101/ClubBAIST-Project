using ClubBAIST.Data;
using ClubBAIST.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubBAIST.Controllers
{
    public class FinanceController : Controller
    {
        private readonly ClubBAISTContext _context;

        public FinanceController(ClubBAISTContext context)
        {
            _context = context;
        }

        // GET: /Finance/MyAccount?memberNumber=M001
        public IActionResult MyAccount(string? memberNumber)
        {
            if (string.IsNullOrEmpty(memberNumber))
                return View("SelectMember");

            var member = _context.Members
                .Include(m => m.MembershipType)
                    .ThenInclude(t => t.Category)
                .Include(m => m.Account)
                    .ThenInclude(a => a.Transactions)
                .FirstOrDefault(m => m.MemberNumber == memberNumber
                                  && m.IsActive);

            if (member == null)
            {
                ViewBag.Error = $"Member '{memberNumber}' not found.";
                return View("SelectMember");
            }

            // Create account if it doesn't exist
            if (member.Account == null)
            {
                var account = new MemberAccount
                {
                    MemberId = member.MemberId,
                    Balance = 0,
                    CreatedAt = DateTime.Now
                };
                _context.MemberAccounts.Add(account);
                _context.SaveChanges();

                // Reload
                member = _context.Members
                    .Include(m => m.MembershipType)
                        .ThenInclude(t => t.Category)
                    .Include(m => m.Account)
                        .ThenInclude(a => a.Transactions)
                    .First(m => m.MemberId == member.MemberId);
            }

            ViewBag.MemberNumber = memberNumber;
            return View(member);
        }

        // GET: /Finance/Accounts (Admin — all accounts)
        public IActionResult Accounts()
        {
            var members = _context.Members
                .Include(m => m.MembershipType)
                    .ThenInclude(t => t.Category)
                .Include(m => m.Account)
                .Where(m => m.IsActive)
                .OrderBy(m => m.LastName)
                .ToList();

            return View(members);
        }

        // GET: /Finance/AddTransaction?memberNumber=M001
        [HttpGet]
        public IActionResult AddTransaction(string memberNumber)
        {
            var member = _context.Members
                .Include(m => m.Account)
                .FirstOrDefault(m => m.MemberNumber == memberNumber
                                  && m.IsActive);

            if (member == null) return NotFound();

            ViewBag.MemberNumber = memberNumber;
            ViewBag.MemberName = member.FullName;
            return View();
        }

        // POST: /Finance/AddTransaction
        [HttpPost]
        public IActionResult AddTransaction(
            string memberNumber,
            string transactionType,
            decimal amount,
            string? description,
            string postedByMemberNumber)
        {
            var member = _context.Members
                .Include(m => m.Account)
                .FirstOrDefault(m => m.MemberNumber == memberNumber
                                  && m.IsActive);

            if (member?.Account == null) return NotFound();

            var postedBy = _context.Members
                .FirstOrDefault(m => m.MemberNumber == postedByMemberNumber);

            var transaction = new AccountTransaction
            {
                AccountId = member.Account.AccountId,
                TransactionDate = DateTime.Today,
                TransactionType = transactionType,
                Amount = amount,
                Description = description,
                PostedByMemberId = postedBy?.MemberId
            };

            _context.AccountTransactions.Add(transaction);

            // Update balance
            // Payments reduce balance, charges increase it
            if (transactionType == "Payment")
                member.Account.Balance -= amount;
            else
                member.Account.Balance += amount;

            _context.SaveChanges();

            TempData["Success"] = $"Transaction posted: {transactionType} ${amount:N2}";
            return RedirectToAction("MyAccount",
                new { memberNumber });
        }

        // POST: /Finance/AssessAnnualFees
        // Admin function — assess annual fees for all members
        [HttpPost]
        public IActionResult AssessAnnualFees(string postedByMemberNumber)
        {
            var members = _context.Members
                .Include(m => m.MembershipType)
                .Include(m => m.Account)
                .Where(m => m.IsActive && m.MembershipType.HasGolfPrivileges)
                .ToList();

            int count = 0;
            foreach (var member in members)
            {
                if (member.Account == null) continue;

                var annualFee = member.MembershipType?.AnnualFee ?? 0;

                // Check if already assessed this year
                var alreadyAssessed = _context.AccountTransactions
                    .Any(t => t.AccountId == member.Account.AccountId
                           && t.TransactionType == "AnnualFee"
                           && t.TransactionDate.Year == DateTime.Today.Year);

                if (alreadyAssessed) continue;

                // Check for late payment penalty (after April 1)
                var isPastDue = DateTime.Today >
                    new DateTime(DateTime.Today.Year, 4, 1);
                var penalty = isPastDue ? annualFee * 0.10m : 0;

                _context.AccountTransactions.Add(new AccountTransaction
                {
                    AccountId = member.Account.AccountId,
                    TransactionDate = DateTime.Today,
                    TransactionType = "AnnualFee",
                    Amount = annualFee,
                    Description = $"Annual membership fee {DateTime.Today.Year}"
                });

                member.Account.Balance += annualFee;

                if (penalty > 0)
                {
                    _context.AccountTransactions.Add(new AccountTransaction
                    {
                        AccountId = member.Account.AccountId,
                        TransactionDate = DateTime.Today,
                        TransactionType = "Penalty",
                        Amount = penalty,
                        Description = $"10% late payment penalty {DateTime.Today.Year}"
                    });

                    member.Account.Balance += penalty;
                }

                count++;
            }

            _context.SaveChanges();

            TempData["Success"] = $"Annual fees assessed for {count} members.";
            return RedirectToAction("Accounts");
        }
    }
}