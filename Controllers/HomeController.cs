using ClubBAIST.Data;
using Microsoft.AspNetCore.Mvc;

namespace ClubBAIST.Controllers
{
    public class HomeController : Controller
    {
        private readonly ClubBAISTContext _context;

        public HomeController(ClubBAISTContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}