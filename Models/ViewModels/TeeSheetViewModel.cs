namespace ClubBAIST.Models.ViewModels
{
    public class TeeSheetViewModel
    {
        public int TeeSheetId { get; set; }
        public DateTime SheetDate { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public List<TeeSlotViewModel> Slots { get; set; } = new();
    }

    public class TeeSlotViewModel
    {
        public int SlotId { get; set; }
        public string SlotTime { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool IsBlocked { get; set; }
        public string? BlockReason { get; set; }

        // Reservation info if booked
        public int? ReservationId { get; set; }
        public string? BookedByName { get; set; }
        public int? NumberOfPlayers { get; set; }
        public int? NumberOfCarts { get; set; }
        public string? ReservationStatus { get; set; }
        public List<string> PlayerNames { get; set; } = new();

        public bool IsBooked => ReservationId != null && ReservationStatus == "Active";
    }
}