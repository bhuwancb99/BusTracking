namespace BusTracking.Web.Areas.Student.Controllers
{
    [Area("Student"), Authorize(Roles = "Student")]
    public class TrackingController : Controller
    {
        private readonly ITripService _trip;

        public TrackingController(ITripService t) => _trip = t;

        public async Task<IActionResult> Track(int tripId = 0)
        {
            var s = await _trip.GetTripStudentsAsync(tripId);
            var l = await _trip.GetLatestLocationAsync(tripId);
            ViewBag.TripId = tripId; ViewBag.Location = l.Data;
            return View(s.Data);
        }
    }
}
