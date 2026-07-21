namespace BusTracking.Common.Services
{
    public class TripService : ITripService
    {
        private readonly AppDbContext _db;
        public TripService(AppDbContext db) => _db = db;

        public async Task<ApiResponse<PagedResult<TripListDto>>> GetAllAsync(int page, string? busId, string? status = null, string? date = null)
        {
            var q = _db.BusTrips
                .IgnoreQueryFilters()
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busId) && int.TryParse(busId, out var bid))
                q = q.Where(t => t.BusId == bid);

            if (!string.IsNullOrWhiteSpace(status) && status != "All" && Enum.TryParse<TripStatus>(status, true, out var st))
                q = q.Where(t => t.Status == st);

            if (DateOnly.TryParse(date, out var d))
                q = q.Where(t => t.TripDate == d);

            var pageSize = await GetListPageSizeAsync();
            page = PaginationHelper.Clamp(page);

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(t => t.TripDate).ThenBy(t => t.TripType)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(t => new TripListDto
                {
                    TripId = t.TripId,
                    BusNumber = t.Bus.BusNumber,
                    DriverName = t.Driver.FullName,
                    RouteName = t.Route.RouteName,
                    StartStopName = t.Route.Stops.Where(s => s.IsActive).OrderBy(s => s.StopOrder).Select(s => s.StopName).FirstOrDefault(),
                    EndStopName = t.Route.Stops.Where(s => s.IsActive).OrderBy(s => s.StopOrder).Select(s => s.StopName).LastOrDefault(),
                    TripType = t.TripType.ToString(),
                    TripDate = t.TripDate.ToString("yyyy-MM-dd"),
                    Status = t.Status.ToString(),
                    StartedAt = t.StartedAt,
                    EndedAt = t.EndedAt
                }).ToListAsync();

            return ApiResponse<PagedResult<TripListDto>>.Ok(new PagedResult<TripListDto>
            { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }

        public Task<int> GetListPageSizeAsync() => PaginationHelper.GetListPageSizeAsync(_db);

        public async Task<ApiResponse<List<StudentTripStatusDto>>> GetTripStudentsAsync(int tripId)
        {
            var trip = await _db.BusTrips
                .IgnoreQueryFilters()
                .Include(t => t.Bus)
                .FirstOrDefaultAsync(t => t.TripId == tripId);
            if (trip is null) return ApiResponse<List<StudentTripStatusDto>>.Fail("Trip not found.");

            var students = await _db.Students
                .IgnoreQueryFilters()
                .Include(s => s.User).Include(s => s.Stop)
                .Where(s => s.BusId == trip.BusId && s.User.IsActive).ToListAsync();

            var statuses = await _db.StudentTripStatuses
                .IgnoreQueryFilters()
                .Where(sts => sts.TripId == tripId).ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var avails = await _db.StudentAvailabilities
                .IgnoreQueryFilters()
                .Where(a => students.Select(s => s.StudentId).Contains(a.StudentId)
                         && today >= a.FromDate && today <= a.ToDate).ToListAsync();

            var result = students.OrderBy(s => s.Stop?.StopOrder ?? 999).ThenBy(s => s.User.FullName)
                .Select(s =>
                {
                    var sts = statuses.FirstOrDefault(x => x.StudentId == s.StudentId);
                    var avail = avails.FirstOrDefault(a => a.StudentId == s.StudentId);
                    return new StudentTripStatusDto
                    {
                        StudentId = s.StudentId,
                        StudentCode = s.StudentCode,
                        StudentName = s.User.FullName,
                        StopName = s.Stop?.StopName ?? "–",
                        StopOrder = s.Stop?.StopOrder ?? 0,
                        BoardingStatus = sts?.BoardingStatus.ToString()
                                         ?? (avail != null ? "OnLeave" : "Pending"),
                        IsUnavailable = avail != null
                    };
                }).ToList();

            return ApiResponse<List<StudentTripStatusDto>>.Ok(result);
        }

        public async Task<ApiResponse<BusLocationDto?>> GetLatestLocationAsync(int tripId)
        {
            var loc = await _db.BusLiveLocations
                .IgnoreQueryFilters()
                .Where(l => l.TripId == tripId)
                .OrderByDescending(l => l.RecordedAt)
                .Select(l => new BusLocationDto
                {
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Speed = l.Speed,
                    Heading = l.Heading,
                    RecordedAt = l.RecordedAt
                }).FirstOrDefaultAsync();
            return ApiResponse<BusLocationDto?>.Ok(loc);
        }

        public async Task<ApiResponse<List<BusLocationDto>>> GetLocationHistoryAsync(int tripId)
        {
            var list = await _db.BusLiveLocations
                .IgnoreQueryFilters()
                .Where(l => l.TripId == tripId)
                .OrderBy(l => l.RecordedAt)
                .Select(l => new BusLocationDto
                {
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Speed = l.Speed,
                    Heading = l.Heading,
                    RecordedAt = l.RecordedAt
                }).ToListAsync();
            return ApiResponse<List<BusLocationDto>>.Ok(list);
        }

        public async Task<ApiResponse<TripListDto>> GetByIdAsync(int tripId)
        {
            var t = await _db.BusTrips
                .IgnoreQueryFilters()
                .Include(x => x.Bus).Include(x => x.Driver)
                .Include(x => x.Route).ThenInclude(r => r!.Stops)
                .FirstOrDefaultAsync(x => x.TripId == tripId);
            if (t is null) return ApiResponse<TripListDto>.Fail("Trip not found.");
            return ApiResponse<TripListDto>.Ok(new TripListDto
            {
                TripId = t.TripId,
                BusNumber = t.Bus.BusNumber,
                DriverName = t.Driver.FullName,
                RouteName = t.Route.RouteName,
                StartStopName = t.Route.Stops.Where(s => s.IsActive).OrderBy(s => s.StopOrder).Select(s => s.StopName).FirstOrDefault(),
                EndStopName = t.Route.Stops.Where(s => s.IsActive).OrderBy(s => s.StopOrder).Select(s => s.StopName).LastOrDefault(),
                TripType = t.TripType.ToString(),
                TripDate = t.TripDate.ToString("yyyy-MM-dd"),
                Status = t.Status.ToString(),
                StartedAt = t.StartedAt,
                EndedAt = t.EndedAt
            });
        }

        public async Task<ApiResponse<TripListDto>> CreateAsync(CreateTripDto dto, int createdBy)
        {
            if (!Enum.TryParse<TripType>(dto.TripType, out var tt))
                return ApiResponse<TripListDto>.Fail("Invalid trip type.");
            if (!DateOnly.TryParse(dto.TripDate, out var td))
                return ApiResponse<TripListDto>.Fail("Invalid date.");

            if (await _db.BusTrips.IgnoreQueryFilters().AnyAsync(t =>
                t.BusId == dto.BusId && t.TripDate == td && t.TripType == tt &&
                t.Status != TripStatus.Cancelled))
                return ApiResponse<TripListDto>.Fail($"A {dto.TripType} trip for this bus on {dto.TripDate} already exists.");

            var driverId = dto.DriverId;
            if (driverId <= 0)
            {
                var assignedDriver = await _db.DriverDetails
                    .IgnoreQueryFilters()
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.BusId == dto.BusId && d.User.IsActive);
                if (assignedDriver == null)
                    return ApiResponse<TripListDto>.Fail("No active driver is assigned to this bus.");
                driverId = assignedDriver.UserId;
            }

            var trip = new BusTrip
            {
                BusId = dto.BusId,
                DriverId = driverId,
                RouteId = dto.RouteId,
                TripType = tt,
                TripDate = td,
                Status = TripStatus.Scheduled
            };
            _db.BusTrips.Add(trip);
            await _db.SaveChangesAsync();

            var stops = await _db.Stops
                .IgnoreQueryFilters()
                .Where(s => s.RouteId == dto.RouteId && s.IsActive)
                .OrderBy(s => s.StopOrder).ToListAsync();
            foreach (var stop in stops)
                _db.TripStopEvents.Add(new TripStopEvent
                { TripId = trip.TripId, StopId = stop.StopId, Status = TripStopStatus.Pending });

            var students = await _db.Students
                .IgnoreQueryFilters()
                .Include(s => s.User)
                .Where(s => s.BusId == dto.BusId && s.User.IsActive)
                .ToListAsync();
            var bus = await _db.Buses.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.BusId == dto.BusId);
            var school = bus?.SchoolId.HasValue == true
                ? await _db.Schools.Include(s => s.TimeZone).IgnoreQueryFilters().FirstOrDefaultAsync(s => s.SchoolId == bus.SchoolId.Value)
                : null;
            var today = TimeZoneHelper.GetSchoolTodayDate(school);

            foreach (var s in students)
            {
                var isUnavail = await _db.StudentAvailabilities
                    .IgnoreQueryFilters()
                    .AnyAsync(a => a.StudentId == s.StudentId && today >= a.FromDate && today <= a.ToDate);

                _db.StudentTripStatuses.Add(new StudentTripStatus
                {
                    TripId = trip.TripId,
                    StudentId = s.StudentId,
                    StopId = s.StopId ?? 0,
                    BoardingStatus = isUnavail ? BoardingStatus.OnLeave : BoardingStatus.Pending
                });
            }

            await _db.SaveChangesAsync();
            return await GetByIdAsync(trip.TripId);
        }

        public async Task<ApiResponse<bool>> StartTripAsync(int tripId)
        {
            var trip = await _db.BusTrips.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.TripId == tripId);
            if (trip is null) return ApiResponse<bool>.Fail("Trip not found.");
            if (trip.Status != TripStatus.Scheduled) return ApiResponse<bool>.Fail("Trip is not in Scheduled status.");
            trip.Status = TripStatus.InProgress;
            trip.StartedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Trip started.");
        }

        public async Task<ApiResponse<bool>> EndTripAsync(int tripId)
        {
            var trip = await _db.BusTrips.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.TripId == tripId);
            if (trip is null) return ApiResponse<bool>.Fail("Trip not found.");
            if (trip.Status != TripStatus.InProgress) return ApiResponse<bool>.Fail("Trip is not InProgress.");
            trip.Status = TripStatus.Completed;
            trip.EndedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Trip completed.");
        }

        public async Task<ApiResponse<bool>> CancelTripAsync(int tripId)
        {
            var trip = await _db.BusTrips.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.TripId == tripId);
            if (trip is null) return ApiResponse<bool>.Fail("Trip not found.");
            if (trip.Status == TripStatus.Completed) return ApiResponse<bool>.Fail("Cannot cancel completed trip.");
            trip.Status = TripStatus.Cancelled;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Trip cancelled.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int tripId)
        {
            var t = await _db.BusTrips.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.TripId == tripId);
            if (t is null) return ApiResponse<bool>.Fail("Trip not found.");
            if (t.Status == TripStatus.InProgress)
                return ApiResponse<bool>.Fail("Cannot delete an in-progress trip. End or cancel it first.");

            var strategy = _db.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var tx = await _db.Database.BeginTransactionAsync();
                try
                {
                    var events = await _db.TripStopEvents.IgnoreQueryFilters().Where(e => e.TripId == tripId).ToListAsync();
                    if (events.Count > 0) _db.TripStopEvents.RemoveRange(events);

                    var statuses = await _db.StudentTripStatuses.IgnoreQueryFilters().Where(s => s.TripId == tripId).ToListAsync();
                    if (statuses.Count > 0) _db.StudentTripStatuses.RemoveRange(statuses);

                    var locations = await _db.BusLiveLocations.IgnoreQueryFilters().Where(l => l.TripId == tripId).ToListAsync();
                    if (locations.Count > 0) _db.BusLiveLocations.RemoveRange(locations);

                    _db.BusTrips.Remove(t);
                    await _db.SaveChangesAsync();
                    await tx.CommitAsync();

                    return ApiResponse<bool>.Ok(true, "Trip deleted successfully.");
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    return ApiResponse<bool>.Fail($"Failed to delete trip: {ex.Message}");
                }
            });
        }

        public async Task<ApiResponse<List<TripStopEventDto>>> GetStopEventsAsync(int tripId)
        {
            var events = await _db.TripStopEvents
                .IgnoreQueryFilters()
                .Include(e => e.Stop)
                .Where(e => e.TripId == tripId)
                .OrderBy(e => e.Stop.StopOrder)
                .Select(e => new TripStopEventDto
                {
                    TripStopEventId = e.TripStopEventId,
                    StopId = e.StopId,
                    StopName = e.Stop.StopName,
                    StopOrder = e.Stop.StopOrder,
                    Status = e.Status.ToString(),
                    ReachedAt = e.ReachedAt,
                    DepartedAt = e.DepartedAt,
                    Latitude = (double?)e.Stop.Latitude,
                    Longitude = (double?)e.Stop.Longitude
                }).ToListAsync();
            return ApiResponse<List<TripStopEventDto>>.Ok(events);
        }

        public async Task<ApiResponse<bool>> ReachStopAsync(int tripId, int stopId)
        {
            var trip = await _db.BusTrips
                .IgnoreQueryFilters()
                .Include(t => t.Bus).ThenInclude(b => b!.Route).ThenInclude(r => r!.Stops)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip?.Bus?.Route is null)
                return ApiResponse<bool>.Fail("Trip or route not found.");

            var orderedStops = trip.Bus.Route.Stops.Where(s => s.IsActive).OrderBy(s => s.StopOrder).ToList();
            var currentStop = orderedStops.FirstOrDefault(s => s.StopId == stopId);
            if (currentStop is null) return ApiResponse<bool>.Fail("Stop not found.");

            // Sequential check: Previous stops must be Departed!
            var previousStopIds = orderedStops.Where(s => s.StopOrder < currentStop.StopOrder).Select(s => s.StopId).ToList();
            if (previousStopIds.Count > 0)
            {
                var prevEvents = await _db.TripStopEvents
                    .IgnoreQueryFilters()
                    .Where(e => e.TripId == tripId && previousStopIds.Contains(e.StopId))
                    .ToListAsync();

                var incompletePrevious = previousStopIds.Any(id =>
                {
                    var e = prevEvents.FirstOrDefault(x => x.StopId == id);
                    return e == null || e.Status != TripStopStatus.Departed;
                });

                if (incompletePrevious)
                {
                    return ApiResponse<bool>.Fail("Cannot reach this stop. All previous stops must be departed first in sequential order.");
                }
            }

            var evt = await _db.TripStopEvents
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.TripId == tripId && e.StopId == stopId);

            if (evt is null)
            {
                _db.TripStopEvents.Add(new TripStopEvent
                { TripId = tripId, StopId = stopId, ReachedAt = DateTime.UtcNow, Status = TripStopStatus.Reached });
            }
            else
            {
                evt.ReachedAt = DateTime.UtcNow;
                evt.Status = TripStopStatus.Reached;
            }
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Stop marked as reached.");
        }

        public async Task<ApiResponse<bool>> DepartStopAsync(int tripId, int stopId)
        {
            var evt = await _db.TripStopEvents
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.TripId == tripId && e.StopId == stopId);

            if (evt is null || evt.Status != TripStopStatus.Reached)
            {
                return ApiResponse<bool>.Fail("Stop must be marked as Reached before marking as Departed.");
            }

            // Sequential check: All students assigned to this stop must have their boarding status updated!
            var stop = await _db.Stops.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.StopId == stopId);
            if (stop != null)
            {
                var stopStudents = await _db.Students
                    .IgnoreQueryFilters()
                    .Where(s => s.StopId == stopId || (s.Stop != null && s.Stop.StopOrder == stop.StopOrder))
                    .Select(s => s.StudentId)
                    .ToListAsync();

                if (stopStudents.Count > 0)
                {
                    var pendingCount = await _db.StudentTripStatuses
                        .IgnoreQueryFilters()
                        .CountAsync(sts => sts.TripId == tripId && stopStudents.Contains(sts.StudentId) && sts.BoardingStatus == BoardingStatus.Pending);

                    if (pendingCount > 0)
                    {
                        return ApiResponse<bool>.Fail("Cannot depart this stop yet. All student boarding statuses (Picked Up, No-Show, or On Leave) for this stop must be updated first.");
                    }
                }
            }

            evt.DepartedAt = DateTime.UtcNow;
            evt.Status = TripStopStatus.Departed;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Stop marked as departed.");
        }

        public async Task<ApiResponse<bool>> UpdateBoardingAsync(int tripId, int studentId, int stopId, string status)
        {
            if (!Enum.TryParse<BoardingStatus>(status, true, out var bs))
                return ApiResponse<bool>.Fail("Invalid status.");

            var s = await _db.StudentTripStatuses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.TripId == tripId && x.StudentId == studentId);
            if (s is null)
            {
                _db.StudentTripStatuses.Add(new StudentTripStatus
                { TripId = tripId, StudentId = studentId, StopId = stopId, BoardingStatus = bs });
            }
            else
            {
                s.BoardingStatus = bs;
                s.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Boarding status updated.");
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
