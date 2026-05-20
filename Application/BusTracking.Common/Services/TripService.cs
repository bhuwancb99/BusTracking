using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Trip;
using BusTracking.Common.Entities;
using BusTracking.Common.Enums;
using BusTracking.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTracking.Common.Services
{
    public class TripService : ITripService
    {
        private readonly AppDbContext _db;
        public TripService(AppDbContext db) => _db = db;

        public async Task<ApiResponse<PagedResult<TripListDto>>> GetAllAsync(int page, int pageSize, string? busId)
        {
            var q = _db.BusTrips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busId) && int.TryParse(busId, out var bid))
                q = q.Where(t => t.BusId == bid);

            var total = await q.CountAsync();
            var items = await q.OrderByDescending(t => t.TripDate).ThenBy(t => t.TripType)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(t => new TripListDto
                {
                    TripId = t.TripId,
                    BusNumber = t.Bus.BusNumber,
                    DriverName = t.Driver.FullName,
                    RouteName = t.Route.RouteName,
                    TripType = t.TripType.ToString(),
                    TripDate = t.TripDate.ToString("yyyy-MM-dd"),
                    Status = t.Status.ToString(),
                    StartedAt = t.StartedAt,
                    EndedAt = t.EndedAt
                }).ToListAsync();

            return ApiResponse<PagedResult<TripListDto>>.Ok(new PagedResult<TripListDto>
            { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }

        public async Task<ApiResponse<List<StudentTripStatusDto>>> GetTripStudentsAsync(int tripId)
        {
            var trip = await _db.BusTrips.Include(t => t.Bus)
                .FirstOrDefaultAsync(t => t.TripId == tripId);
            if (trip is null) return ApiResponse<List<StudentTripStatusDto>>.Fail("Trip not found.");

            var students = await _db.Students
                .Include(s => s.User).Include(s => s.Stop)
                .Where(s => s.BusId == trip.BusId && s.User.IsActive).ToListAsync();

            var statuses = await _db.StudentTripStatuses
                .Where(sts => sts.TripId == tripId).ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var avails = await _db.StudentAvailabilities
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


        public async Task<ApiResponse<TripListDto>> GetByIdAsync(int tripId)
        {
            var t = await _db.BusTrips
                .Include(x => x.Bus).Include(x => x.Driver).Include(x => x.Route)
                .FirstOrDefaultAsync(x => x.TripId == tripId);
            if (t is null) return ApiResponse<TripListDto>.Fail("Trip not found.");
            return ApiResponse<TripListDto>.Ok(new TripListDto
            {
                TripId = t.TripId,
                BusNumber = t.Bus.BusNumber,
                DriverName = t.Driver.FullName,
                RouteName = t.Route.RouteName,
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

            // Check duplicate trip same bus+date+type
            if (await _db.BusTrips.AnyAsync(t =>
                t.BusId == dto.BusId && t.TripDate == td && t.TripType == tt &&
                t.Status != TripStatus.Cancelled))
                return ApiResponse<TripListDto>.Fail($"A {dto.TripType} trip for this bus on {dto.TripDate} already exists.");

            var trip = new BusTrip
            {
                BusId = dto.BusId,
                DriverId = dto.DriverId,
                RouteId = dto.RouteId,
                TripType = tt,
                TripDate = td,
                Status = TripStatus.Scheduled
            };
            _db.BusTrips.Add(trip);
            await _db.SaveChangesAsync();

            // Auto-create TripStopEvents for all stops on route
            var stops = await _db.Stops
                .Where(s => s.RouteId == dto.RouteId && s.IsActive)
                .OrderBy(s => s.StopOrder).ToListAsync();
            foreach (var stop in stops)
                _db.TripStopEvents.Add(new TripStopEvent
                { TripId = trip.TripId, StopId = stop.StopId, Status = TripStopStatus.Pending });

            // Auto-create StudentTripStatus for all students on this bus
            var students = await _db.Students
                .Include(s => s.User)
                .Where(s => s.BusId == dto.BusId && s.User.IsActive)
                .ToListAsync();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            foreach (var s in students)
            {
                var onLeave = await _db.StudentAvailabilities.AnyAsync(a =>
                    a.StudentId == s.StudentId && today >= a.FromDate && today <= a.ToDate);
                _db.StudentTripStatuses.Add(new StudentTripStatus
                {
                    TripId = trip.TripId,
                    StudentId = s.StudentId,
                    StopId = s.StopId ?? (stops.FirstOrDefault()?.StopId ?? 0),
                    BoardingStatus = onLeave ? BoardingStatus.OnLeave : BoardingStatus.Pending
                });
            }
            await _db.SaveChangesAsync();

            return await GetByIdAsync(trip.TripId);
        }

        public async Task<ApiResponse<bool>> StartTripAsync(int tripId)
        {
            var t = await _db.BusTrips.FindAsync(tripId);
            if (t is null) return ApiResponse<bool>.Fail("Trip not found.");
            if (t.Status != TripStatus.Scheduled)
                return ApiResponse<bool>.Fail($"Cannot start a trip with status: {t.Status}.");
            t.Status = TripStatus.InProgress;
            t.StartedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Trip started.");
        }

        public async Task<ApiResponse<bool>> EndTripAsync(int tripId)
        {
            var t = await _db.BusTrips.FindAsync(tripId);
            if (t is null) return ApiResponse<bool>.Fail("Trip not found.");
            t.Status = TripStatus.Completed;
            t.EndedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Trip completed.");
        }

        public async Task<ApiResponse<bool>> CancelTripAsync(int tripId)
        {
            var t = await _db.BusTrips.FindAsync(tripId);
            if (t is null) return ApiResponse<bool>.Fail("Trip not found.");
            t.Status = TripStatus.Cancelled;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Trip cancelled.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int tripId)
        {
            var t = await _db.BusTrips.FindAsync(tripId);
            if (t is null) return ApiResponse<bool>.Fail("Trip not found.");
            if (t.Status == TripStatus.InProgress)
                return ApiResponse<bool>.Fail("Cannot delete an in-progress trip. End or cancel it first.");
            _db.BusTrips.Remove(t);
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Trip deleted.");
        }

        public async Task<ApiResponse<bool>> UpdateBoardingAsync(int tripId, int studentId, int stopId, string status)
        {
            if (!Enum.TryParse<BoardingStatus>(status, out var bs))
                return ApiResponse<bool>.Fail("Invalid boarding status.");
            var existing = await _db.StudentTripStatuses
                .FirstOrDefaultAsync(s => s.TripId == tripId && s.StudentId == studentId);
            if (existing is null)
                _db.StudentTripStatuses.Add(new StudentTripStatus
                { TripId = tripId, StudentId = studentId, StopId = stopId, BoardingStatus = bs });
            else
            { existing.BoardingStatus = bs; existing.UpdatedAt = DateTime.UtcNow; }
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, $"Marked as {status}.");
        }

        public async Task<ApiResponse<bool>> ReachStopAsync(int tripId, int stopId)
        {
            var evt = await _db.TripStopEvents
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

        public async Task<ApiResponse<List<TripStopEventDto>>> GetStopEventsAsync(int tripId)
        {
            var events = await _db.TripStopEvents
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
                    DepartedAt = e.DepartedAt
                }).ToListAsync();
            return ApiResponse<List<TripStopEventDto>>.Ok(events);
        }

        public async Task<ApiResponse<bool>> InsertLocationPingAsync(
        int tripId, int busId, decimal lat, decimal lng, decimal? speed, decimal? heading)
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
            return ApiResponse<bool>.Ok(true);
        }

    }
}
