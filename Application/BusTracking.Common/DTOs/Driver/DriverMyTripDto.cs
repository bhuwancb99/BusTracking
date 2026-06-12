namespace BusTracking.Common.DTOs.Driver
{
    public class DriverMyTripDto
    {
        // ── Bus info ──────────────────────────────────────────────────────
        public int? BusId { get; set; }
        public string BusName { get; set; } = "";
        public string BusNumber { get; set; } = "";

        // ── Route info ────────────────────────────────────────────────────
        public int? RouteId { get; set; }
        public string RouteName { get; set; } = "";

        // ── Today's trip (null = no trip scheduled today) ─────────────────
        public DriverTripSummary? Trip { get; set; }
    }
}
