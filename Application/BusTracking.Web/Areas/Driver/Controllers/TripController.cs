namespace BusTracking.Web.Areas.Driver.Controllers;

[Area("Driver"), Authorize(Roles = "Driver")]
public class TripController : Controller
{
    private readonly IDriverTripWebService _driverTrip;
    private readonly IAppConfigService _appConfig;

    private int CurrentUserId => int.TryParse(
        User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public TripController(IDriverTripWebService driverTrip, IAppConfigService appConfig)
    {
        _driverTrip = driverTrip;
        _appConfig = appConfig;
    }

    // GET /Driver/Trip/Index  — today's trip dashboard card
    public async Task<IActionResult> Index()
    {
        var r = await _driverTrip.GetMyTripAsync(CurrentUserId);
        return View(r.Data);
    }

    // GET /Driver/Trip/Details/{tripId} — full map + stops + students view
    public async Task<IActionResult> Details(int tripId)
    {
        var myTrip = await _driverTrip.GetMyTripAsync(CurrentUserId);
        if (myTrip.Data?.Trip is null || myTrip.Data.Trip.TripId != tripId)
            return RedirectToAction(nameof(Index));

        var stops = await _driverTrip.GetTripStopsAsync(tripId);
        var students = await _driverTrip.GetTripStudentsAsync(tripId);
        ViewBag.Stops = stops.Data ?? [];
        ViewBag.Students = students.Data ?? [];
        ViewBag.GoogleMapApiKey = await _appConfig.GetValueAsync("GoogleMapApiKey") ?? "";
        var rawHubUrl = await _appConfig.GetValueAsync(AppConstants.AppConfigTrackingHubUrlKey);
        ViewBag.TrackingHubUrl = AppConstants.FormatTrackingHubUrl(rawHubUrl);
        ViewBag.BusInfo = myTrip.Data;
        return View(myTrip.Data.Trip);
    }

    // POST /Driver/Trip/Start/{tripId}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int tripId)
    {
        var r = await _driverTrip.StartTripAsync(tripId, CurrentUserId);
        TempData[r.Success ? "SuccessMessage" : "ErrorMessage"] = r.Message;
        if (r.Success)
            return RedirectToAction(nameof(Details), new { tripId });
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

    // POST /Driver/Trip/ReachStop — AJAX
    [HttpPost]
    public async Task<IActionResult> ReachStop(int tripId, int stopId)
    {
        var r = await _driverTrip.ReachStopAsync(tripId, stopId);
        return Json(new { r.Success, r.Message });
    }

    // POST /Driver/Trip/DepartStop — AJAX
    [HttpPost]
    public async Task<IActionResult> DepartStop(int tripId, int stopId)
    {
        var r = await _driverTrip.DepartStopAsync(tripId, stopId);
        return Json(new { r.Success, r.Message });
    }

    // POST /Driver/Trip/UpdateBoarding — AJAX
    [HttpPost]
    public async Task<IActionResult> UpdateBoarding([FromBody] UpdateBoardingRequest req)
    {
        var r = await _driverTrip.UpdateBoardingAsync(req.TripId, req.StudentId, req.StopId, req.Status);
        return Json(new { r.Success, r.Message });
    }

    // GET /Driver/Trip/LatestLocation — AJAX
    [HttpGet]
    public async Task<IActionResult> LatestLocation(int tripId)
    {
        var r = await _driverTrip.GetLatestLocationAsync(tripId);
        return Json(new { success = r.Success, data = r.Data });
    }

    // GET /Driver/Trip/LocationHistory — AJAX
    [HttpGet]
    public async Task<IActionResult> LocationHistory(int tripId)
    {
        var r = await _driverTrip.GetLocationHistoryAsync(tripId);
        return Json(new { success = r.Success, data = r.Data });
    }

    // POST /Driver/Trip/PingLocation — AJAX (from browser geolocation)
    [HttpPost]
    public async Task<IActionResult> PingLocation([FromBody] LocationPingDto dto)
    {
        var r = await _driverTrip.InsertLocationPingAsync(
            dto.TripId, dto.BusId, dto.Latitude, dto.Longitude, dto.Speed, dto.Heading);
        return Json(new { r.Success, r.Message });
    }

    // Legacy: kept for backward compat
    public async Task<IActionResult> Students(int tripId)
    {
        var r = await _driverTrip.GetTripStudentsAsync(tripId);
        ViewBag.TripId = tripId;
        return View(r.Data);
    }

    public async Task<IActionResult> Stops(int tripId)
    {
        var r = await _driverTrip.GetTripStopsAsync(tripId);
        ViewBag.TripId = tripId;
        return View(r.Data);
    }
}
