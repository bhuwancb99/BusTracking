using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Trip;
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
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var trip = await _db.BusTrips.Include(t => t.Bus).FirstOrDefaultAsync(t => t.TripId == tripId);
            if (trip is null) return ApiResponse<List<StudentTripStatusDto>>.Fail("Trip not found.");

            var students = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Stop)
                .Where(s => s.BusId == trip.BusId && s.User.IsActive)
                .ToListAsync();

            var tripStatuses = await _db.StudentTripStatuses
                .Where(sts => sts.TripId == tripId)
                .ToListAsync();

            var availabilities = await _db.StudentAvailabilities
                .Where(a => a.FromDate <= today && a.ToDate >= today
                         && students.Select(s => s.StudentId).Contains(a.StudentId))
                .ToListAsync();

            var result = students
                .OrderBy(s => s.Stop?.StopOrder ?? 999)
                .ThenBy(s => s.User.FullName)
                .Select(s =>
                {
                    var sts = tripStatuses.FirstOrDefault(x => x.StudentId == s.StudentId);
                    var avail = availabilities.FirstOrDefault(a => a.StudentId == s.StudentId);
                    var boarding = sts?.BoardingStatus.ToString()
                        ?? (avail is not null ? "OnLeave" : "Pending");

                    return new StudentTripStatusDto
                    {
                        StudentId = s.StudentId,
                        StudentCode = s.StudentCode,
                        StudentName = s.User.FullName,
                        StopName = s.Stop?.StopName ?? "–",
                        StopOrder = s.Stop?.StopOrder ?? 0,
                        BoardingStatus = boarding,
                        IsUnavailable = avail is not null
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
    }
}
