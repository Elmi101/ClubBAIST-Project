using ClubBAIST.Data;
using ClubBAIST.Models.Domain;
using ClubBAIST.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClubBAIST.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ClubBAISTContext _context;

        public ReservationController(ClubBAISTContext context)
        {
            _context = context;
        }

        // GET: /Reservation/Book?slotId=5
        [HttpGet]
        public IActionResult Book(int slotId)
        {
            var slot = _context.TeeTimeSlots
                .Include(s => s.TeeSheet)
                .FirstOrDefault(s => s.SlotId == slotId);

            if (slot == null)
                return NotFound("Tee time slot not found.");

            if (!slot.IsAvailable || slot.IsBlocked)
                return BadRequest("This slot is not available.");

            var model = new BookReservationViewModel
            {
                SlotId = slotId,
                SlotTime = DateTime.Today.Add(slot.SlotTime).ToString("h:mm tt"),
                SheetDate = slot.TeeSheet.SheetDate
            };

            return View(model);
        }

        // POST: /Reservation/Book
        [HttpPost]
        public IActionResult Book(BookReservationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Find the member booking
            var bookedBy = _context.Members
                .Include(m => m.MembershipType)
                .FirstOrDefault(m => m.MemberNumber == model.BookedByMemberNumber
                                  && m.IsActive);

            if (bookedBy == null)
            {
                model.ErrorMessage = "Member number not found.";
                return View(model);
            }

            if (!bookedBy.IsGoodStanding)
            {
                model.ErrorMessage = "Member is not in good standing and cannot book a tee time.";
                return View(model);
            }

            if (bookedBy.MembershipType?.HasGolfPrivileges == false)
            {
                model.ErrorMessage = "This member type does not have golf privileges.";
                return View(model);
            }

            // Get the slot
            var slot = _context.TeeTimeSlots
                .Include(s => s.TeeSheet)
                .Include(s => s.Reservation)
                .FirstOrDefault(s => s.SlotId == model.SlotId);

            if (slot == null || !slot.IsAvailable || slot.IsBlocked)
            {
                model.ErrorMessage = "This slot is no longer available.";
                return View(model);
            }


            if (slot.Reservation != null && slot.Reservation.Status == "Active")
            {
                model.ErrorMessage = "This slot has already been booked.";
                return View(model);
            }

            // Clean up any old cancelled reservations and their players for this slot
            if (slot.Reservation != null && slot.Reservation.Status == "Cancelled")
            {
                var oldPlayers = _context.ReservationPlayers
                    .Where(p => p.ReservationId == slot.Reservation.ReservationId)
                    .ToList();
                _context.ReservationPlayers.RemoveRange(oldPlayers);
                _context.TeeTimeReservations.Remove(slot.Reservation);
                _context.SaveChanges();
            }

            // Check booking window (max 1 week in advance)
            var daysInAdvance = (slot.TeeSheet.SheetDate.Date - DateTime.Today).TotalDays;
            if (daysInAdvance > 7)
            {
                model.ErrorMessage = "Tee times can only be booked up to 1 week in advance.";
                return View(model);
            }

            // Validate tee time restriction by membership type
            var restriction = bookedBy.MembershipType?.TeeTimeRestriction;
            var slotTime = slot.SlotTime;
            var dayOfWeek = slot.TeeSheet.SheetDate.DayOfWeek;
            bool isWeekend = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;

            if (restriction == "Silver")
            {
                bool allowed = isWeekend
                    ? slotTime >= new TimeSpan(11, 0, 0)
                    : slotTime < new TimeSpan(15, 0, 0) || slotTime >= new TimeSpan(17, 30, 0);

                if (!allowed)
                {
                    model.ErrorMessage = "Silver members: weekdays before 3PM or after 5:30PM, weekends after 11AM.";
                    return View(model);
                }
            }
            else if (restriction == "Bronze")
            {
                bool allowed = isWeekend
                    ? slotTime >= new TimeSpan(13, 0, 0)
                    : slotTime < new TimeSpan(15, 0, 0) || slotTime >= new TimeSpan(18, 0, 0);

                if (!allowed)
                {
                    model.ErrorMessage = "Bronze members: weekdays before 3PM or after 6PM, weekends after 1PM.";
                    return View(model);
                }
            }

            // Create the reservation
            var reservation = new TeeTimeReservation
            {
                SlotId = model.SlotId,
                BookedByMemberId = bookedBy.MemberId,
                NumberOfPlayers = model.NumberOfPlayers,
                NumberOfCarts = model.NumberOfCarts,
                Status = "Active",
                BookedAt = DateTime.Now
            };

            _context.TeeTimeReservations.Add(reservation);
            _context.SaveChanges();

            // Add players
            // Always add the booking member as player 1
            _context.ReservationPlayers.Add(new ReservationPlayer
            {
                ReservationId = reservation.ReservationId,
                MemberId = bookedBy.MemberId
            });

            // Add additional players
            AddPlayer(reservation.ReservationId, model.Player2MemberNumber, model.Player2GuestName);
            AddPlayer(reservation.ReservationId, model.Player3MemberNumber, model.Player3GuestName);
            AddPlayer(reservation.ReservationId, model.Player4MemberNumber, model.Player4GuestName);

            // Mark slot as no longer available
            slot.IsAvailable = false;
            _context.SaveChanges();

            TempData["Success"] = $"Tee time booked for {bookedBy.FullName} at {DateTime.Today.Add(slot.SlotTime):h:mm tt}";
            return RedirectToAction("ViewSheet", "TeeSheet",
                new { date = slot.TeeSheet.SheetDate.ToString("yyyy-MM-dd") });
        }

        private void AddPlayer(int reservationId, string? memberNumber, string? guestName)
        {
            if (string.IsNullOrWhiteSpace(memberNumber) && string.IsNullOrWhiteSpace(guestName))
                return;

            var player = new ReservationPlayer { ReservationId = reservationId };

            if (!string.IsNullOrWhiteSpace(memberNumber))
            {
                var member = _context.Members
                    .FirstOrDefault(m => m.MemberNumber == memberNumber && m.IsActive);
                if (member != null)
                    player.MemberId = member.MemberId;
                else
                    player.GuestName = memberNumber; // treat as guest name if not found
            }
            else
            {
                player.GuestName = guestName;
            }

            _context.ReservationPlayers.Add(player);
        }

        // GET: /Reservation/Cancel?reservationId=3
        [HttpGet]
        public IActionResult Cancel(int reservationId)
        {
            var reservation = _context.TeeTimeReservations
                .Include(r => r.BookedBy)
                .Include(r => r.Slot)
                    .ThenInclude(s => s.TeeSheet)
                .Include(r => r.Players)
                    .ThenInclude(p => p.Member)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
                return NotFound();

            return View(reservation);
        }

        // GET: /Reservation/Modify?reservationId=3
        [HttpGet]
        public IActionResult Modify(int reservationId)
        {
            var reservation = _context.TeeTimeReservations
                .Include(r => r.Slot)
                    .ThenInclude(s => s.TeeSheet)
                .Include(r => r.BookedBy)
                .Include(r => r.Players)
                    .ThenInclude(p => p.Member)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
                return NotFound();

            var model = new BookReservationViewModel
            {
                SlotId = reservation.SlotId,
                SlotTime = DateTime.Today.Add(reservation.Slot.SlotTime)
                                   .ToString("h:mm tt"),
                SheetDate = reservation.Slot.TeeSheet.SheetDate,
                NumberOfPlayers = reservation.NumberOfPlayers,
                NumberOfCarts = reservation.NumberOfCarts ?? 0,
                BookedByMemberNumber = reservation.BookedBy?.MemberNumber
                                       ?? string.Empty
            };

            // Pre-fill existing players
            var players = reservation.Players.ToList();
            if (players.Count > 1)
            {
                var p2 = players[1];
                model.Player2MemberNumber = p2.Member?.MemberNumber;
                model.Player2GuestName = p2.GuestName;
            }
            if (players.Count > 2)
            {
                var p3 = players[2];
                model.Player3MemberNumber = p3.Member?.MemberNumber;
                model.Player3GuestName = p3.GuestName;
            }
            if (players.Count > 3)
            {
                var p4 = players[3];
                model.Player4MemberNumber = p4.Member?.MemberNumber;
                model.Player4GuestName = p4.GuestName;
            }

            ViewBag.ReservationId = reservationId;
            return View(model);
        }

        // POST: /Reservation/Modify
        [HttpPost]
        public IActionResult Modify(
            int reservationId,
            int numberOfPlayers,
            int numberOfCarts,
            string? player2MemberNumber, string? player2GuestName,
            string? player3MemberNumber, string? player3GuestName,
            string? player4MemberNumber, string? player4GuestName)
        {
            var reservation = _context.TeeTimeReservations
                .Include(r => r.Slot)
                    .ThenInclude(s => s.TeeSheet)
                .Include(r => r.Players)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
                return NotFound();

            // Update reservation
            reservation.NumberOfPlayers = numberOfPlayers;
            reservation.NumberOfCarts = numberOfCarts;

            // Remove old players except player 1 (the booker)
            var oldPlayers = reservation.Players
                .Skip(1).ToList();
            _context.ReservationPlayers.RemoveRange(oldPlayers);
            _context.SaveChanges();

            // Add updated players
            AddPlayer(reservation.ReservationId,
                      player2MemberNumber, player2GuestName);
            AddPlayer(reservation.ReservationId,
                      player3MemberNumber, player3GuestName);
            AddPlayer(reservation.ReservationId,
                      player4MemberNumber, player4GuestName);

            _context.SaveChanges();

            TempData["Success"] =
                $"Reservation updated successfully.";

            return RedirectToAction("ViewSheet", "TeeSheet",
                new
                {
                    date = reservation.Slot.TeeSheet.SheetDate
                                         .ToString("yyyy-MM-dd")
                });
        }

        // POST: /Reservation/Cancel
        [HttpPost]
        public IActionResult CancelConfirmed(int reservationId, string cancelledByMemberNumber)
        {
            // Validate the member exists
            if (string.IsNullOrWhiteSpace(cancelledByMemberNumber))
            {
                TempData["Error"] = "Please enter your member number to cancel.";
                return RedirectToAction("Cancel", new { reservationId });
            }

            var cancelledBy = _context.Members
                .FirstOrDefault(m => m.MemberNumber == cancelledByMemberNumber && m.IsActive);

            if (cancelledBy == null)
            {
                TempData["Error"] = $"Member number '{cancelledByMemberNumber}' not found.";
                return RedirectToAction("Cancel", new { reservationId });
            }

            var reservation = _context.TeeTimeReservations
                .Include(r => r.Slot)
                    .ThenInclude(s => s.TeeSheet)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
                return NotFound();

            reservation.Status = "Cancelled";
            reservation.CancelledAt = DateTime.Now;
            reservation.CancelledByMemberId = cancelledBy.MemberId;

            // Mark slot available again
            reservation.Slot.IsAvailable = true;

            _context.SaveChanges();

            TempData["Success"] = $"Reservation cancelled by {cancelledBy.FullName}.";
            return RedirectToAction("ViewSheet", "TeeSheet",
                new
                {
                    date = reservation.Slot.TeeSheet?.SheetDate.ToString("yyyy-MM-dd")
                      ?? DateTime.Today.ToString("yyyy-MM-dd")
                });
        }
        // GET: /Reservation/CheckIn?reservationId=3
        [HttpGet]
        public IActionResult CheckIn(int reservationId)
        {
            var reservation = _context.TeeTimeReservations
                .Include(r => r.BookedBy)
                .Include(r => r.Slot)
                    .ThenInclude(s => s.TeeSheet)
                .Include(r => r.Players)
                    .ThenInclude(p => p.Member)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (reservation == null)
                return NotFound();

            return View(reservation);
        }

        // POST: /Reservation/CheckIn
        [HttpPost]
        public IActionResult CheckInConfirmed(int reservationId, List<int> checkedInPlayerIds)
        {
            var players = _context.ReservationPlayers
                .Where(p => p.ReservationId == reservationId)
                .ToList();

            foreach (var player in players)
            {
                player.HasCheckedIn = checkedInPlayerIds.Contains(player.ReservationPlayerId);
            }

            _context.SaveChanges();

            TempData["Success"] = "Check-in updated.";
            return RedirectToAction("ViewSheet", "TeeSheet",
                new { date = DateTime.Today.ToString("yyyy-MM-dd") });
        }
    }
}