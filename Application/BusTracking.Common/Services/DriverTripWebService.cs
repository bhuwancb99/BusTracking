namespace BusTracking.Common.Services
{

    public class DriverTripWebService : IDriverTripWebService
    {
        private readonly AppDbContext _db;
        public DriverTripWebService(AppDbContext db) => _db = db;

        // ── My Trip ────────────────────────────────────────────────────────

        public async Task<ApiResponse<DriverMyTripDto>> GetMyTripAsync(int driverUserId)
        {
            var driver = await _db.DriverDetails
                .Include(d => d.Bus).ThenInclude(b => b!.Route)
                .Include(d => d.User).ThenInclude(u => u!.School).ThenInclude(s => s!.TimeZone)
                .FirstOrDefaultAsync(d => d.UserId == driverUserId);

            if (driver?.Bus is null)
                return ApiResponse<DriverMyTripDto>.Fail("No bus assigned to this driver.");

            var dto = new DriverMyTripDto
            {
                BusId = driver.Bus.BusId,
                BusName = driver.Bus.BusName,
                BusNumber = driver.Bus.BusNumber,
                RouteId = driver.Bus.Route?.RouteId,
                RouteName = driver.Bus.Route?.RouteName ?? "No route assigned"
            };

            var schoolToday = TimeZoneHelper.GetSchoolTodayDate(driver.User?.School);
            var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);

            var trip = await _db.BusTrips
                .FirstOrDefaultAsync(t => t.BusId == driver.BusId && t.Status == TripStatus.InProgress)
                    ?? await _db.BusTrips
                .FirstOrDefaultAsync(t => t.BusId == driver.BusId
                                       && (t.TripDate == schoolToday || t.TripDate == todayUtc)
                                       && t.Status != TripStatus.Cancelled);

            if (trip is not null)
            {
                dto.Trip = new DriverTripSummary
                {
                    TripId = trip.TripId,
                    TripType = trip.TripType.ToString(),
                    TripDate = trip.TripDate.ToString("yyyy-MM-dd"),
                    Status = trip.Status.ToString(),
                    StartedAt = trip.StartedAt,
                    EndedAt = trip.EndedAt
                };
            }

            return ApiResponse<DriverMyTripDto>.Ok(dto);
        }

        // ── Start / End ────────────────────────────────────────────────────

        public async Task<ApiResponse<bool>> StartTripAsync(int tripId, int driverUserId)
        {
            var trip = await _db.BusTrips.FindAsync(tripId);
            if (trip is null || trip.DriverId != driverUserId)
                return ApiResponse<bool>.Fail("Trip not found.");
            if (trip.Status != TripStatus.Scheduled)
                return ApiResponse<bool>.Fail("Trip cannot be started in its current status.");

            trip.Status = TripStatus.InProgress;
            trip.StartedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Trip started successfully.");
        }

        public async Task<ApiResponse<bool>> EndTripAsync(int tripId, int driverUserId)
        {
            var trip = await _db.BusTrips.FindAsync(tripId);
            if (trip is null || trip.DriverId != driverUserId)
                return ApiResponse<bool>.Fail("Trip not found.");
            if (trip.Status != TripStatus.InProgress)
                return ApiResponse<bool>.Fail("Trip is not currently in progress.");

            trip.Status = TripStatus.Completed;
            trip.EndedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Trip completed successfully.");
        }

        // ── Students ───────────────────────────────────────────────────────

        public async Task<ApiResponse<List<StudentTripStatusDto>>> GetTripStudentsAsync(int tripId)
        {
            var trip = await _db.BusTrips.Include(t => t.Bus)
                .FirstOrDefaultAsync(t => t.TripId == tripId);
            if (trip is null)
                return ApiResponse<List<StudentTripStatusDto>>.Fail("Trip not found.");

            var students = await _db.Students
                .Include(s => s.User).Include(s => s.Stop)
                .Where(s => s.BusId == trip.BusId && s.User.IsActive)
                .ToListAsync();

            var statuses = await _db.StudentTripStatuses
                .Where(x => x.TripId == tripId).ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var studentIds = students.Select(s => s.StudentId).ToList();
            var unavail = await _db.StudentAvailabilities
                .Where(a => studentIds.Contains(a.StudentId)
                         && today >= a.FromDate && today <= a.ToDate)
                .Select(a => a.StudentId).ToListAsync();

            var result = students
                .OrderBy(s => s.Stop?.StopOrder ?? 999)
                .ThenBy(s => s.User.FullName)
                .Select(s =>
                {
                    var sts = statuses.FirstOrDefault(x => x.StudentId == s.StudentId);
                    return new StudentTripStatusDto
                    {
                        StudentId = s.StudentId,
                        StudentCode = s.StudentCode,
                        StudentName = s.User.FullName,
                        StopName = s.Stop?.StopName ?? "—",
                        StopOrder = s.Stop?.StopOrder ?? 0,
                        BoardingStatus = sts?.BoardingStatus.ToString() ?? "Pending",
                        IsUnavailable = unavail.Contains(s.StudentId)
                    };
                }).ToList();

            return ApiResponse<List<StudentTripStatusDto>>.Ok(result);
        }

        // ── Stops ──────────────────────────────────────────────────────────

        public async Task<ApiResponse<List<TripStopEventDto>>> GetTripStopsAsync(int tripId)
        {
            var trip = await _db.BusTrips
                .Include(t => t.Bus).ThenInclude(b => b!.Route)
                    .ThenInclude(r => r!.Stops)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip?.Bus?.Route is null)
                return ApiResponse<List<TripStopEventDto>>.Fail("Trip or route not found.");

            var events = await _db.TripStopEvents
                .Where(e => e.TripId == tripId)
                .ToListAsync();
            var eventsByStop = events.ToDictionary(e => e.StopId);

            var stops = trip.Bus.Route.Stops
                .OrderBy(s => s.StopOrder)
                .Select(s =>
                {
                    eventsByStop.TryGetValue(s.StopId, out var ev);
                    return new TripStopEventDto
                    {
                        TripStopEventId = ev?.TripStopEventId ?? 0,
                        StopId = s.StopId,
                        StopName = s.StopName,
                        StopOrder = s.StopOrder,
                        Status = ev?.Status.ToString() ?? "Pending",
                        ReachedAt = ev?.ReachedAt,
                        DepartedAt = ev?.DepartedAt
                    };
                }).ToList();

            return ApiResponse<List<TripStopEventDto>>.Ok(stops);
        }

        // ── Stop events ────────────────────────────────────────────────────

        public async Task<ApiResponse<bool>> ReachStopAsync(int tripId, int stopId)
        {
            var ev = await _db.TripStopEvents
                .FirstOrDefaultAsync(e => e.TripId == tripId && e.StopId == stopId);

            if (ev is null)
                _db.TripStopEvents.Add(new TripStopEvent
                {
                    TripId = tripId,
                    StopId = stopId,
                    ReachedAt = DateTime.UtcNow,
                    Status = TripStopStatus.Reached
                });
            else
            {
                ev.ReachedAt = DateTime.UtcNow;
                ev.Status = TripStopStatus.Reached;
            }

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Stop marked as reached.");
        }

        public async Task<ApiResponse<bool>> DepartStopAsync(int tripId, int stopId)
        {
            var ev = await _db.TripStopEvents
                .FirstOrDefaultAsync(e => e.TripId == tripId && e.StopId == stopId);

            if (ev is null)
                _db.TripStopEvents.Add(new TripStopEvent
                {
                    TripId = tripId,
                    StopId = stopId,
                    DepartedAt = DateTime.UtcNow,
                    Status = TripStopStatus.Departed
                });
            else
            {
                ev.DepartedAt = DateTime.UtcNow;
                ev.Status = TripStopStatus.Departed;
            }

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Stop marked as departed.");
        }
    }
}
