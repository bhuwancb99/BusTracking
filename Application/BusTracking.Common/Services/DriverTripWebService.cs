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

            var timeZoneId = driver.User?.School?.TimeZoneInfoId
                          ?? driver.User?.School?.TimeZone?.WindowsTimeZoneId
                          ?? "India Standard Time";
            var schoolToday = TimeZoneHelper.GetToday(timeZoneId);

            // Auto-close lingering InProgress trips from previous days
            var yesterdayInProgress = await _db.BusTrips
                .Where(t => t.BusId == driver.BusId && t.Status == TripStatus.InProgress && t.TripDate < schoolToday)
                .ToListAsync();

            if (yesterdayInProgress.Count > 0)
            {
                foreach (var oldTrip in yesterdayInProgress)
                {
                    oldTrip.Status = TripStatus.Completed;
                    oldTrip.EndedAt = TimeZoneHelper.GetNow(timeZoneId);
                }
                await _db.SaveChangesAsync();
            }

            // Fetch today's trip
            var trip = await _db.BusTrips
                .FirstOrDefaultAsync(t => t.BusId == driver.BusId
                                       && t.TripDate == schoolToday
                                       && t.Status == TripStatus.InProgress)
                    ?? await _db.BusTrips
                .FirstOrDefaultAsync(t => t.BusId == driver.BusId
                                       && t.TripDate == schoolToday
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

            var timeZoneId = _db.Users
                .Include(u => u.School)
                .Where(u => u.UserId == driverUserId)
                .Select(u => u.School != null ? u.School.TimeZoneInfoId : null)
                .FirstOrDefault() ?? "India Standard Time";

            trip.Status = TripStatus.InProgress;
            trip.StartedAt = TimeZoneHelper.GetNow(timeZoneId);
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

            var timeZoneId = _db.Users
                .Include(u => u.School)
                .Where(u => u.UserId == driverUserId)
                .Select(u => u.School != null ? u.School.TimeZoneInfoId : null)
                .FirstOrDefault() ?? "India Standard Time";

            trip.Status = TripStatus.Completed;
            trip.EndedAt = TimeZoneHelper.GetNow(timeZoneId);
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

            var timeZoneId = _db.Users
                .Include(u => u.School)
                .Where(u => u.UserId == trip.DriverId)
                .Select(u => u.School != null ? u.School.TimeZoneInfoId : null)
                .FirstOrDefault() ?? "India Standard Time";
            var today = TimeZoneHelper.GetToday(timeZoneId);

            var studentIds = students.Select(s => s.StudentId).ToList();
            var unavail = await _db.StudentAvailabilities
                .Where(a => studentIds.Contains(a.StudentId) && today >= a.FromDate && today <= a.ToDate)
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
                .Include(t => t.Bus).ThenInclude(b => b!.Route).ThenInclude(r => r!.Stops)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip?.Bus?.Route is null)
                return ApiResponse<List<TripStopEventDto>>.Fail("Trip or route not found.");

            var events = await _db.TripStopEvents
                .Where(e => e.TripId == tripId).ToListAsync();
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
            // Sequential check: previous stops must be Departed
            var trip = await _db.BusTrips
                .Include(t => t.Bus).ThenInclude(b => b!.Route).ThenInclude(r => r!.Stops)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip?.Bus?.Route is not null)
            {
                var orderedStops = trip.Bus.Route.Stops.Where(s => s.IsActive).OrderBy(s => s.StopOrder).ToList();
                var currentStop = orderedStops.FirstOrDefault(s => s.StopId == stopId);
                if (currentStop is not null)
                {
                    var prevStopIds = orderedStops.Where(s => s.StopOrder < currentStop.StopOrder).Select(s => s.StopId).ToList();
                    if (prevStopIds.Count > 0)
                    {
                        var prevEvents = await _db.TripStopEvents
                            .Where(e => e.TripId == tripId && prevStopIds.Contains(e.StopId)).ToListAsync();
                        var incomplete = prevStopIds.Any(id =>
                        {
                            var e = prevEvents.FirstOrDefault(x => x.StopId == id);
                            return e == null || e.Status != TripStopStatus.Departed;
                        });
                        if (incomplete)
                            return ApiResponse<bool>.Fail("Cannot reach this stop. All previous stops must be departed first in sequential order.");
                    }
                }
            }

            var ev = await _db.TripStopEvents.FirstOrDefaultAsync(e => e.TripId == tripId && e.StopId == stopId);

            var timeZoneId = _db.BusTrips
                .Include(t => t.Driver).ThenInclude(d => d!.School)
                .Where(t => t.TripId == tripId)
                .Select(t => t.Driver.School != null ? t.Driver.School.TimeZoneInfoId : null)
                .FirstOrDefault() ?? "India Standard Time";
            var now = TimeZoneHelper.GetNow(timeZoneId);

            if (ev is null)
                _db.TripStopEvents.Add(new TripStopEvent
                {
                    TripId = tripId,
                    StopId = stopId,
                    ReachedAt = now,
                    Status = TripStopStatus.Reached
                });
            else
            {
                ev.ReachedAt = now;
                ev.Status = TripStopStatus.Reached;
            }

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Stop marked as reached.");
        }

        public async Task<ApiResponse<bool>> DepartStopAsync(int tripId, int stopId)
        {
            var ev = await _db.TripStopEvents.FirstOrDefaultAsync(e => e.TripId == tripId && e.StopId == stopId);
            if (ev is null || ev.Status != TripStopStatus.Reached)
                return ApiResponse<bool>.Fail("Stop must be marked as Reached before marking as Departed.");

            // Check all students at this stop have updated boarding status
            var stop = await _db.Stops.FirstOrDefaultAsync(s => s.StopId == stopId);
            if (stop != null)
            {
                var stopStudentIds = await _db.Students
                    .Where(s => s.StopId == stopId).Select(s => s.StudentId).ToListAsync();
                if (stopStudentIds.Count > 0)
                {
                    var pendingCount = await _db.StudentTripStatuses
                        .CountAsync(sts => sts.TripId == tripId && stopStudentIds.Contains(sts.StudentId) && sts.BoardingStatus == BoardingStatus.Pending);
                    if (pendingCount > 0)
                        return ApiResponse<bool>.Fail("Cannot depart this stop yet. All student boarding statuses (Picked Up, No-Show, or On Leave) for this stop must be updated first.");
                }
            }

            var timeZoneId = _db.BusTrips
                .Include(t => t.Driver).ThenInclude(d => d!.School)
                .Where(t => t.TripId == tripId)
                .Select(t => t.Driver.School != null ? t.Driver.School.TimeZoneInfoId : null)
                .FirstOrDefault() ?? "India Standard Time";
            var now = TimeZoneHelper.GetNow(timeZoneId);

            ev.DepartedAt = now;
            ev.Status = TripStopStatus.Departed;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Stop marked as departed.");
        }

        // ── Boarding status ────────────────────────────────────────────────
        public async Task<ApiResponse<bool>> UpdateBoardingAsync(int tripId, int studentId, int stopId, string status)
        {
            if (!Enum.TryParse<BoardingStatus>(status, true, out var bs))
                return ApiResponse<bool>.Fail("Invalid boarding status.");

            var existing = await _db.StudentTripStatuses
                .FirstOrDefaultAsync(x => x.TripId == tripId && x.StudentId == studentId);

            if (existing is null)
                _db.StudentTripStatuses.Add(new StudentTripStatus { TripId = tripId, StudentId = studentId, StopId = stopId, BoardingStatus = bs });
            else { existing.BoardingStatus = bs; existing.UpdatedAt = DateTime.UtcNow; }

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Boarding status updated.");
        }

        // ── Location ───────────────────────────────────────────────────────
        public async Task<ApiResponse<BusLocationDto?>> GetLatestLocationAsync(int tripId)
        {
            var loc = await _db.BusLiveLocations
                .Where(l => l.TripId == tripId)
                .OrderByDescending(l => l.RecordedAt)
                .Select(l => new BusLocationDto { Latitude = l.Latitude, Longitude = l.Longitude, Speed = l.Speed, Heading = l.Heading, RecordedAt = l.RecordedAt })
                .FirstOrDefaultAsync();
            return ApiResponse<BusLocationDto?>.Ok(loc);
        }

        public async Task<ApiResponse<List<BusLocationDto>>> GetLocationHistoryAsync(int tripId)
        {
            var list = await _db.BusLiveLocations
                .Where(l => l.TripId == tripId)
                .OrderBy(l => l.RecordedAt)
                .Select(l => new BusLocationDto { Latitude = l.Latitude, Longitude = l.Longitude, Speed = l.Speed, Heading = l.Heading, RecordedAt = l.RecordedAt })
                .ToListAsync();
            return ApiResponse<List<BusLocationDto>>.Ok(list);
        }

        public async Task<ApiResponse<bool>> InsertLocationPingAsync(int tripId, int busId, decimal lat, decimal lng, decimal? speed, decimal? heading)
        {
            _db.BusLiveLocations.Add(new BusLiveLocation
            {
                TripId = tripId,
                BusId = busId,
                Latitude = lat,
                Longitude = lng,
                Speed = speed,
                Heading = heading,
                RecordedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Location recorded.");
        }
    }
}
