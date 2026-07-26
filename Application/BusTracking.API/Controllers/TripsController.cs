namespace BusTracking.API.Controllers
{
    [Authorize(Roles = "Driver"), Route("api/[controller]")]
    public class TripsController : ApiBaseController
    {
        private readonly AppDbContext _db;
        private readonly ITripService _trip;
        public TripsController(AppDbContext db, ITripService trip) { _db = db; _trip = trip; }

        private DateTime GetSchoolNow()
        {
            return TimeZoneHelper.GetNow(CurrentTimeZoneInfoId);
        }

        /// <summary>Get driver's assigned bus and today's trip</summary>
        [HttpGet("my-trip")]
        public async Task<IActionResult> GetMyTrip()
        {
            var mapping = await _db.BusDriverMappings
                .Include(dm => dm.Bus)
                .FirstOrDefaultAsync(dm => dm.DriverUserId == CurrentUserId);

            if (mapping?.Bus is null)
                return NotFound(ApiResponse<object>.Fail("No bus assigned."));

            var user = await _db.Users.Include(u => u.School).ThenInclude(s => s!.TimeZone).FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
            var schoolToday = TimeZoneHelper.GetSchoolTodayDate(user?.School);
            var todayUtc = DateOnly.FromDateTime(GetSchoolNow());

            var busId = mapping.Bus.BusId;
            var trip = await _db.BusTrips
                .Include(t => t.Route)
                .FirstOrDefaultAsync(t => t.BusId == busId && t.Status == TripStatus.InProgress)
                    ?? await _db.BusTrips
                .Include(t => t.Route)
                .FirstOrDefaultAsync(t => t.BusId == busId
                                       && (t.TripDate == schoolToday || t.TripDate == todayUtc)
                                       && t.Status != TripStatus.Cancelled);

            var totalStudents = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Stop)
                .CountAsync(s => s.User.IsActive && s.BusId == busId);

            return Ok(ApiResponse<object>.Ok(new
            {
                Bus = new
                {
                    mapping.Bus.BusId,
                    mapping.Bus.BusName,
                    mapping.Bus.BusNumber
                },
                Route = trip?.Route is null ? null : new
                {
                    trip.Route.RouteId,
                    trip.Route.RouteName
                },
                TotalStudents = totalStudents,
                Trip = trip is null ? null : new
                {
                    trip.TripId,
                    TripType = trip.TripType.ToString(),
                    Status = trip.Status.ToString(),
                    trip.StartedAt,
                    trip.EndedAt
                }
            }));
        }

        /// <summary>Start a trip</summary>
        [HttpPost("{tripId}/start")]
        public async Task<IActionResult> Start(int tripId)
        {
            var r = await _trip.StartTripAsync(tripId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>End a trip</summary>
        [HttpPost("{tripId}/end")]
        public async Task<IActionResult> End(int tripId)
        {
            var trip = await _db.BusTrips.FindAsync(tripId);
            if (trip is null || trip.DriverId != CurrentUserId)
                return NotFound(ApiResponse<bool>.Fail("Trip not found."));

            var now = GetSchoolNow();
            trip.Status = TripStatus.Completed;
            trip.EndedAt = now;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Trip completed."));
        }

        /// <summary>Get students list for a trip (with availability)</summary>
        [HttpGet("{tripId}/students")]
        public async Task<IActionResult> GetStudents(int tripId)
        {
            var r = await _trip.GetTripStudentsAsync(tripId);
            return Ok(r);
        }

        /// <summary>Mark a stop as reached (Step-by-Step validation)</summary>
        [HttpPost("{tripId}/stops/{stopId}/reach")]
        public async Task<IActionResult> ReachStop(int tripId, int stopId)
        {
            var trip = await _db.BusTrips
                .Include(t => t.Route).ThenInclude(r => r!.Stops)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip?.Route is null)
                return NotFound(ApiResponse<bool>.Fail("Trip or route not found."));

            var orderedStops = trip.Route.Stops.Where(s => s.IsActive).OrderBy(s => s.StopOrder).ToList();
            var currentStop = orderedStops.FirstOrDefault(s => s.StopId == stopId);
            if (currentStop is null) return NotFound(ApiResponse<bool>.Fail("Stop not found."));

            // Sequential check: Previous stops must be Departed!
            var previousStopIds = orderedStops.Where(s => s.StopOrder < currentStop.StopOrder).Select(s => s.StopId).ToList();
            if (previousStopIds.Count > 0)
            {
                var prevEvents = await _db.TripStopEvents
                    .Where(e => e.TripId == tripId && previousStopIds.Contains(e.StopId))
                    .ToListAsync();

                var incompletePrevious = previousStopIds.Any(id =>
                {
                    var e = prevEvents.FirstOrDefault(x => x.StopId == id);
                    return e == null || e.Status != TripStopStatus.Departed;
                });

                if (incompletePrevious)
                {
                    return BadRequest(ApiResponse<bool>.Fail("Cannot reach this stop. All previous stops must be departed first in sequential order."));
                }
            }

            var evt = await _db.TripStopEvents
                .FirstOrDefaultAsync(e => e.TripId == tripId && e.StopId == stopId);

            var now = GetSchoolNow();

            if (evt is null)
            {
                _db.TripStopEvents.Add(new TripStopEvent
                {
                    TripId = tripId,
                    StopId = stopId,
                    ReachedAt = now,
                    Status = TripStopStatus.Reached
                });
            }
            else
            {
                evt.ReachedAt = now;
                evt.Status = TripStopStatus.Reached;
            }

            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Stop marked as reached."));
        }

        /// <summary>Mark a stop as departed (Step-by-Step validation)</summary>
        [HttpPost("{tripId}/stops/{stopId}/depart")]
        public async Task<IActionResult> DepartStop(int tripId, int stopId)
        {
            var evt = await _db.TripStopEvents
                .FirstOrDefaultAsync(e => e.TripId == tripId && e.StopId == stopId);

            if (evt is null || evt.Status != TripStopStatus.Reached)
            {
                return BadRequest(ApiResponse<bool>.Fail("Stop must be marked as Reached before marking as Departed."));
            }

            var now = GetSchoolNow();
            evt.DepartedAt = now;
            evt.Status = TripStopStatus.Departed;

            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Stop marked as departed."));
        }

        /// <summary>Get all stops with status for a trip</summary>
        [HttpGet("{tripId}/stops")]
        public async Task<IActionResult> GetStops(int tripId)
        {
            var trip = await _db.BusTrips
                .Include(t => t.Route).ThenInclude(r => r!.Stops)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip?.Route is null)
                return NotFound(ApiResponse<object>.Fail("Trip or route not found."));

            var events = await _db.TripStopEvents
                .Where(e => e.TripId == tripId)
                .ToDictionaryAsync(e => e.StopId);

            var stops = trip.Route.Stops
                .OrderBy(s => s.StopOrder)
                .Select(s => new
                {
                    s.StopId,
                    s.StopName,
                    s.StopOrder,
                    s.Latitude,
                    s.Longitude,
                    Status = events.TryGetValue(s.StopId, out var e) ? e.Status.ToString() : "Pending",
                    ReachedAt = events.TryGetValue(s.StopId, out var e2) ? e2.ReachedAt : null,
                    DepartedAt = events.TryGetValue(s.StopId, out var e3) ? e3.DepartedAt : null
                }).ToList();

            return Ok(ApiResponse<object>.Ok(stops));
        }

        public class SosAlertRequest
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Message { get; set; } = "EMERGENCY SOS ALERT!";
        }

        /// <summary>Driver triggers SOS Emergency Alert</summary>
        [HttpPost("{tripId}/sos")]
        public async Task<IActionResult> TriggerSos(int tripId, [FromBody] SosAlertRequest req)
        {
            var trip = await _db.BusTrips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip is null)
                return NotFound(ApiResponse<bool>.Fail("Trip not found."));

            var now = GetSchoolNow();
            var msgText = $"🚨 EMERGENCY SOS: Driver {trip.Driver?.FullName ?? "Driver"} triggered SOS on Bus {trip.Bus?.BusNumber ?? ""}. Location: {req.Latitude:F5}, {req.Longitude:F5}. {req.Message}";

            // Notify all Coordinators & SuperAdmins
            var adminUsers = await _db.Users
                .Where(u => u.RoleId == 1 || u.RoleId == 2)
                .Select(u => u.UserId)
                .ToListAsync();

            foreach (var uid in adminUsers)
            {
                _db.Notifications.Add(new Notification
                {
                    RecipientUserId = uid,
                    NotificationType = NotificationType.SOSAlert,
                    Title = "🚨 EMERGENCY SOS ALERT",
                    Body = msgText,
                    SentAt = now,
                    IsRead = false
                });
            }

            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "SOS Emergency Alert broadcasted successfully."));
        }
    }
}
