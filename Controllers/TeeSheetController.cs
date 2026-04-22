using ClubBAIST.Data;
using ClubBAIST.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubBAIST.Controllers
{
    public class TeeSheetController : Controller
    {
        private readonly ClubBAISTContext _context;

        public TeeSheetController(ClubBAISTContext context)
        {
            _context = context;
        }

        // GET: /TeeSheet/Index
        public IActionResult Index()
        {
            var todayDate = DateTime.Today.ToString("yyyy-MM-dd");
            return RedirectToAction("ViewSheet", new { date = todayDate });
        }

        // GET: /TeeSheet/ViewSheet?date=2026-03-08
        public IActionResult ViewSheet(string date)
        {
            if (!DateTime.TryParse(date, out DateTime sheetDate))
                sheetDate = DateTime.Today;

            var teeSheet = _context.TeeSheets
                .Include(ts => ts.Slots)
                    .ThenInclude(s => s.Reservation)
                        .ThenInclude(r => r.BookedBy)
                .Include(ts => ts.Slots)
                    .ThenInclude(s => s.Reservation)
                        .ThenInclude(r => r.Players)
                            .ThenInclude(p => p.Member)
                .FirstOrDefault(ts => ts.SheetDate == sheetDate.Date);

            if (teeSheet == null)
            {
                ViewBag.Message = $"No tee sheet found for {sheetDate:MMMM dd, yyyy}.";
                ViewBag.Date = sheetDate;
                return View("NoTeeSheet");
            }

            var viewModel = new TeeSheetViewModel
            {
                TeeSheetId = teeSheet.TeeSheetId,
                SheetDate = teeSheet.SheetDate,
                DayOfWeek = teeSheet.SheetDate.DayOfWeek.ToString(),
                IsLocked = teeSheet.IsLocked,
                Slots = teeSheet.Slots.OrderBy(s => s.SlotTime).Select(s =>
                {
                    var slot = new TeeSlotViewModel
                    {
                        SlotId = s.SlotId,
                        SlotTime = DateTime.Today.Add(s.SlotTime).ToString("h:mm tt"),
                        IsAvailable = s.IsAvailable,
                        IsBlocked = s.IsBlocked,
                        BlockReason = s.BlockReason,
                        ReservationId = s.Reservation?.ReservationId,
                        ReservationStatus = s.Reservation?.Status,
                        BookedByName = s.Reservation?.BookedBy?.FullName,
                        NumberOfPlayers = s.Reservation?.NumberOfPlayers,
                        NumberOfCarts = s.Reservation?.NumberOfCarts
                    };

                    if (s.Reservation?.Players != null)
                    {
                        foreach (var p in s.Reservation.Players)
                        {
                            slot.PlayerNames.Add(
                                p.Member?.FullName ?? p.GuestName ?? "Unknown");
                        }
                    }

                    return slot;
                }).ToList()
            };

            return View("ViewSheet", viewModel);
        }
    }
}