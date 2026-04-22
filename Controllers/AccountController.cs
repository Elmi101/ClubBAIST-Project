using ClubBAIST.Data;
using ClubBAIST.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClubBAIST.Controllers
{
    public class AccountController : Controller
    {
        private readonly ClubBAISTContext _context;

        public AccountController(ClubBAISTContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to home
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Find member by member number
            var member = _context.Members
                .FirstOrDefault(m => m.MemberNumber == model.MemberNumber
                                  && m.IsActive == true);

            // Check member exists and password matches
            if (member == null || member.PasswordHash != model.Password)
            {
                model.ErrorMessage = "Invalid member number or password.";
                return View(model);
            }

            // Check member is in good standing
            if (!member.IsGoodStanding)
            {
                model.ErrorMessage = "Your account is not in good standing. Please contact the club.";
                return View(model);
            }

            // Build claims (this is what gets stored in the cookie)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, member.MemberId.ToString()),
                new Claim(ClaimTypes.Name, member.FullName),
                new Claim("MemberNumber", member.MemberNumber),
                new Claim(ClaimTypes.Role, member.Role),
                new Claim("MembershipType", member.TypeId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            // Sign in — creates the session cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}