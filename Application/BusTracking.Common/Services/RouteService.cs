namespace BusTracking.Common.Services
{
    public class RouteService : IRouteService
    {
        private readonly AppDbContext _db;
        public RouteService(AppDbContext db) => _db = db;
        public async Task<ApiResponse<PagedResult<RouteListDto>>> GetAllAsync(int page, int pageSize, string? search)
        {
            var q = _db.Routes.Include(r => r.Stops).Where(r => r.IsActive);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(r => r.RouteName.Contains(search) || r.RouteCode.Contains(search));
            var total = await q.CountAsync();
            var items = await q.OrderBy(r => r.RouteName).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => new RouteListDto { RouteId = r.RouteId, RouteName = r.RouteName, RouteCode = r.RouteCode, MorningTime = r.MorningTime != null ? r.MorningTime.Value.ToString("HH:mm") : null, EveningTime = r.EveningTime != null ? r.EveningTime.Value.ToString("HH:mm") : null, StopCount = r.Stops.Count(s => s.IsActive), IsActive = r.IsActive }).ToListAsync();
            return ApiResponse<PagedResult<RouteListDto>>.Ok(new PagedResult<RouteListDto> { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }
        public async Task<ApiResponse<RouteDetailDto>> GetByIdAsync(int routeId)
        {
            var r = await _db.Routes.Include(x => x.Stops.Where(s => s.IsActive)).FirstOrDefaultAsync(x => x.RouteId == routeId && x.IsActive);
            if (r is null) return ApiResponse<RouteDetailDto>.Fail("Not found.");
            return ApiResponse<RouteDetailDto>.Ok(new RouteDetailDto { RouteId = r.RouteId, RouteName = r.RouteName, RouteCode = r.RouteCode, Description = r.Description, MorningTime = r.MorningTime?.ToString("HH:mm"), EveningTime = r.EveningTime?.ToString("HH:mm"), StopCount = r.Stops.Count, IsActive = r.IsActive, Stops = r.Stops.OrderBy(s => s.StopOrder).Select(s => new StopDto { StopId = s.StopId, StopName = s.StopName, StopOrder = s.StopOrder, Latitude = s.Latitude, Longitude = s.Longitude, MorningTime = s.MorningTime?.ToString("HH:mm"), EveningTime = s.EveningTime?.ToString("HH:mm") }).ToList() });
        }
        public async Task<ApiResponse<bool>> CreateAsync(CreateRouteDto dto, int createdBy)
        {
            if (await _db.Routes.AnyAsync(r => r.RouteCode == dto.RouteCode)) return ApiResponse<bool>.Fail("Route code exists.");
            _db.Routes.Add(new BusRoute { RouteName = dto.RouteName, RouteCode = dto.RouteCode.ToUpper(), Description = dto.Description, MorningTime = dto.MorningTime is not null ? TimeOnly.Parse(dto.MorningTime) : null, EveningTime = dto.EveningTime is not null ? TimeOnly.Parse(dto.EveningTime) : null, CreatedBy = createdBy });
            await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Route created.");
        }
        public async Task<ApiResponse<bool>> UpdateAsync(int routeId, UpdateRouteDto dto)
        {
            var r = await _db.Routes.FindAsync(routeId); if (r is null) return ApiResponse<bool>.Fail("Not found.");
            if (await _db.Routes.AnyAsync(x => x.RouteCode == dto.RouteCode && x.RouteId != routeId)) return ApiResponse<bool>.Fail("Route code in use.");
            r.RouteName = dto.RouteName; r.RouteCode = dto.RouteCode.ToUpper(); r.Description = dto.Description;
            r.MorningTime = dto.MorningTime is not null ? TimeOnly.Parse(dto.MorningTime) : null; r.EveningTime = dto.EveningTime is not null ? TimeOnly.Parse(dto.EveningTime) : null; r.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Route updated.");
        }
        public async Task<ApiResponse<bool>> DeleteAsync(int routeId)
        { var r = await _db.Routes.FindAsync(routeId); if (r is null) return ApiResponse<bool>.Fail("Not found."); r.IsActive = false; r.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Deleted."); }
        public async Task<ApiResponse<bool>> AddStopAsync(CreateStopDto dto)
        { _db.Stops.Add(new Stop { RouteId = dto.RouteId, StopName = dto.StopName, StopOrder = dto.StopOrder, Latitude = dto.Latitude, Longitude = dto.Longitude, MorningTime = dto.MorningTime is not null ? TimeOnly.Parse(dto.MorningTime) : null, EveningTime = dto.EveningTime is not null ? TimeOnly.Parse(dto.EveningTime) : null }); await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Stop added."); }
        public async Task<ApiResponse<bool>> DeleteStopAsync(int stopId)
        { var s = await _db.Stops.FindAsync(stopId); if (s is null) return ApiResponse<bool>.Fail("Not found."); s.IsActive = false; await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Removed."); }
        public async Task<ApiResponse<List<StopDto>>> GetStopsByRouteAsync(int routeId)
        {
            var stops = await _db.Stops.Where(s => s.RouteId == routeId && s.IsActive)
                .OrderBy(s => s.StopOrder)
                .Select(s => new StopDto { StopId = s.StopId, StopName = s.StopName, StopOrder = s.StopOrder, Latitude = s.Latitude, Longitude = s.Longitude, MorningTime = s.MorningTime != null ? s.MorningTime.Value.ToString("HH:mm") : null, EveningTime = s.EveningTime != null ? s.EveningTime.Value.ToString("HH:mm") : null })
                .ToListAsync();
            return ApiResponse<List<StopDto>>.Ok(stops);
        }
        public async Task<ApiResponse<List<StopDto>>> GetStopsByBusAsync(int busId)
        {
            var bus = await _db.Buses.Include(b => b.Route).FirstOrDefaultAsync(b => b.BusId == busId);
            if (bus?.RouteId == null) return ApiResponse<List<StopDto>>.Ok([]);
            return await GetStopsByRouteAsync(bus.RouteId.Value);
        }
    }
}
