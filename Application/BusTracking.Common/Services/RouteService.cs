using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Route;
using BusTracking.Common.DTOs.Stop;
using BusTracking.Common.Entities;
using BusTracking.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTracking.Common.Services
{
    public class RouteService : IRouteService
    {
        private readonly AppDbContext _db;
        public RouteService(AppDbContext db) => _db = db;

        public async Task<ApiResponse<PagedResult<RouteListDto>>> GetAllAsync(int page, int pageSize, string? search)
        {
            var q = _db.Routes.Include(r => r.Stops).Where(r => r.IsActive);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(r => r.RouteName.Contains(search) || r.RouteCode.Contains(search));

            var total = await q.CountAsync();
            var items = await q.OrderBy(r => r.RouteName)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => new RouteListDto
                {
                    RouteId = r.RouteId,
                    RouteName = r.RouteName,
                    RouteCode = r.RouteCode,
                    MorningTime = r.MorningTime != null ? r.MorningTime.Value.ToString("HH:mm") : null,
                    EveningTime = r.EveningTime != null ? r.EveningTime.Value.ToString("HH:mm") : null,
                    StopCount = r.Stops.Count(s => s.IsActive),
                    IsActive = r.IsActive
                }).ToListAsync();

            return ApiResponse<PagedResult<RouteListDto>>.Ok(new PagedResult<RouteListDto>
            { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }

        public async Task<ApiResponse<RouteDetailDto>> GetByIdAsync(int routeId)
        {
            var r = await _db.Routes
                .Include(x => x.Stops.Where(s => s.IsActive))
                .FirstOrDefaultAsync(x => x.RouteId == routeId && x.IsActive);
            if (r is null) return ApiResponse<RouteDetailDto>.Fail("Route not found.");

            return ApiResponse<RouteDetailDto>.Ok(new RouteDetailDto
            {
                RouteId = r.RouteId,
                RouteName = r.RouteName,
                RouteCode = r.RouteCode,
                Description = r.Description,
                MorningTime = r.MorningTime?.ToString("HH:mm"),
                EveningTime = r.EveningTime?.ToString("HH:mm"),
                StopCount = r.Stops.Count,
                IsActive = r.IsActive,
                Stops = r.Stops.OrderBy(s => s.StopOrder).Select(s => new StopDto
                {
                    StopId = s.StopId,
                    StopName = s.StopName,
                    StopOrder = s.StopOrder,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    MorningTime = s.MorningTime?.ToString("HH:mm"),
                    EveningTime = s.EveningTime?.ToString("HH:mm")
                }).ToList()
            });
        }

        public async Task<ApiResponse<bool>> CreateAsync(CreateRouteDto dto, int createdBy)
        {
            if (await _db.Routes.AnyAsync(r => r.RouteCode == dto.RouteCode))
                return ApiResponse<bool>.Fail("Route code already exists.");

            _db.Routes.Add(new BusRoute
            {
                RouteName = dto.RouteName,
                RouteCode = dto.RouteCode.ToUpper(),
                Description = dto.Description,
                MorningTime = dto.MorningTime is not null ? TimeOnly.Parse(dto.MorningTime) : null,
                EveningTime = dto.EveningTime is not null ? TimeOnly.Parse(dto.EveningTime) : null,
                CreatedBy = createdBy
            });
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Route created.");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int routeId, UpdateRouteDto dto)
        {
            var route = await _db.Routes.FindAsync(routeId);
            if (route is null) return ApiResponse<bool>.Fail("Route not found.");

            if (await _db.Routes.AnyAsync(r => r.RouteCode == dto.RouteCode && r.RouteId != routeId))
                return ApiResponse<bool>.Fail("Route code already in use.");

            route.RouteName = dto.RouteName;
            route.RouteCode = dto.RouteCode.ToUpper();
            route.Description = dto.Description;
            route.MorningTime = dto.MorningTime is not null ? TimeOnly.Parse(dto.MorningTime) : null;
            route.EveningTime = dto.EveningTime is not null ? TimeOnly.Parse(dto.EveningTime) : null;
            route.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Route updated.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int routeId)
        {
            var route = await _db.Routes.FindAsync(routeId);
            if (route is null) return ApiResponse<bool>.Fail("Route not found.");
            route.IsActive = false;
            route.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Route deleted.");
        }

        public async Task<ApiResponse<bool>> AddStopAsync(CreateStopDto dto)
        {
            _db.Stops.Add(new Stop
            {
                RouteId = dto.RouteId,
                StopName = dto.StopName,
                StopOrder = dto.StopOrder,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                MorningTime = dto.MorningTime is not null ? TimeOnly.Parse(dto.MorningTime) : null,
                EveningTime = dto.EveningTime is not null ? TimeOnly.Parse(dto.EveningTime) : null
            });
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Stop added.");
        }

        public async Task<ApiResponse<bool>> DeleteStopAsync(int stopId)
        {
            var stop = await _db.Stops.FindAsync(stopId);
            if (stop is null) return ApiResponse<bool>.Fail("Stop not found.");
            stop.IsActive = false;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Stop removed.");
        }
    }
}
