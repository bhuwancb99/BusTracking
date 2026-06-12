namespace BusTracking.Web.Areas.Driver.Controllers;

[Area("Driver"), Authorize(Roles = "Driver")]
public class TripController : Controller
{
    private readonly IDriverTripWebService _driverTrip;

    private int CurrentUserId => int.TryParse(
        User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public TripController(IDriverTripWebService driverTrip) => _driverTrip = driverTrip;

    // GET /Driver/Trip/Index  — today's trip + bus/route info
    public async Task<IActionResult> Index()
    {
        var r = await _driverTrip.GetMyTripAsync(CurrentUserId);
        return View(r.Data);
    }

    // POST /Driver/Trip/Start/{tripId}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int tripId)
    {
        var r = await _driverTrip.StartTripAsync(tripId, CurrentUserId);
        TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
        return RedirectToAction(nameof(Index));
    }

    // POST /Driver/Trip/End/{tripId}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> End(int tripId)
    {
        var r = await _driverTrip.EndTripAsync(tripId, CurrentUserId);
        TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
        return RedirectToAction(nameof(Index));
    }

    // GET /Driver/Trip/Students/{tripId}
    public async Task<IActionResult> Students(int tripId)
    {
        var r = await _driverTrip.GetTripStudentsAsync(tripId);
        ViewBag.TripId = tripId;
        return View(r.Data);
    }

    // GET /Driver/Trip/Stops/{tripId}
    public async Task<IActionResult> Stops(int tripId)
    {
        var r = await _driverTrip.GetTripStopsAsync(tripId);
        ViewBag.TripId = tripId;
        return View(r.Data);
    }

    // POST /Driver/Trip/ReachStop
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReachStop(int tripId, int stopId)
    {
        await _driverTrip.ReachStopAsync(tripId, stopId);
        return RedirectToAction(nameof(Stops), new { tripId });
    }

    // POST /Driver/Trip/DepartStop
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DepartStop(int tripId, int stopId)
    {
        await _driverTrip.DepartStopAsync(tripId, stopId);
        return RedirectToAction(nameof(Stops), new { tripId });
    }
}
