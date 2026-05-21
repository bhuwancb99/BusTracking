namespace BusTracking.API.Controllers
{
    [Authorize(Roles = "Driver"), Route("api/[controller]")]
    public class TripsController : ApiBaseController
    {
        private readonly AppDbContext _db;
        private readonly ITripService _trip;
        public TripsController(AppDbContext db, ITripService trip) { _db = db; _trip = trip; }

        /// <summary>Get driver's assigned bus and today's trip</summary>
        [HttpGet("my-trip")]
        public async Task<IActionResult> GetMyTrip()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var driver = await _db.DriverDetails
                .Include(d => d.Bus).ThenInclude(b => b!.Route)
                .FirstOrDefaultAsync(d => d.UserId == CurrentUserId);

            if (driver?.Bus is null)
                return NotFound(ApiResponse<object>.Fail("No bus assigned."));

            var trip = await _db.BusTrips
                .FirstOrDefaultAsync(t => t.BusId == driver.BusId && t.TripDate == today
                                       && t.Status != TripStatus.Cancelled);

            return Ok(ApiResponse<object>.Ok(new
            {
                Bus = new
                {
                    driver.Bus.BusId,
                    driver.Bus.BusName,
                    driver.Bus.BusNumber
                },
                Route = driver.Bus.Route is null ? null : new
                {
                    driver.Bus.Route.RouteId,
                    driver.Bus.Route.RouteName
                },
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
            var trip = await _db.BusTrips.FindAsync(tripId);
            if (trip is null || trip.DriverId != CurrentUserId)
                return NotFound(ApiResponse<bool>.Fail("Trip not found."));

            trip.Status = TripStatus.InProgress;
            trip.StartedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Trip started."));
        }

        /// <summary>End a trip</summary>
        [HttpPost("{tripId}/end")]
        public async Task<IActionResult> End(int tripId)
        {
            var trip = await _db.BusTrips.FindAsync(tripId);
            if (trip is null || trip.DriverId != CurrentUserId)
                return NotFound(ApiResponse<bool>.Fail("Trip not found."));

            trip.Status = TripStatus.Completed;
            trip.EndedAt = DateTime.UtcNow;
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

        /// <summary>Mark a stop as reached</summary>
        [HttpPost("{tripId}/stops/{stopId}/reach")]
        public async Task<IActionResult> ReachStop(int tripId, int stopId)
        {
            var evt = await _db.TripStopEvents
                .FirstOrDefaultAsync(e => e.TripId == tripId && e.StopId == stopId);

            if (evt is null)
            {
                _db.TripStopEvents.Add(new TripStopEvent
                {
                    TripId = tripId,
                    StopId = stopId,
                    ReachedAt = DateTime.UtcNow,
                    Status = TripStopStatus.Reached
                });
            }
            else
            {
                evt.ReachedAt = DateTime.UtcNow;
                evt.Status = TripStopStatus.Reached;
            }

            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Stop marked as reached."));
        }
    }
}
