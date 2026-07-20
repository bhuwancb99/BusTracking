namespace BusTracking.Common.Services
{
    public class RouteService : IRouteService
    {
        private readonly AppDbContext _db;
        public RouteService(AppDbContext db) => _db = db;
        public async Task<ApiResponse<PagedResult<RouteListDto>>> GetAllAsync(int page, string? search, string? status = "Active")
        {
            var q = _db.Routes.Include(r => r.Stops).AsQueryable();
            if (status == "Active") q = q.Where(r => r.IsActive);
            else if (status == "Inactive") q = q.Where(r => !r.IsActive);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(r => r.RouteName.Contains(search) || r.RouteCode.Contains(search));

            var pageSize = await GetListPageSizeAsync();
            page = PaginationHelper.Clamp(page);

            var total = await q.CountAsync();
            var items = await q.OrderBy(r => r.RouteName).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => new RouteListDto
                {
                    RouteId = r.RouteId,
                    RouteName = r.RouteName,
                    RouteCode = r.RouteCode,
                    StartStopName = r.Stops.Where(s => s.IsActive).OrderBy(s => s.StopOrder).Select(s => s.StopName).FirstOrDefault(),
                    EndStopName = r.Stops.Where(s => s.IsActive).OrderBy(s => s.StopOrder).Select(s => s.StopName).LastOrDefault(),
                    MorningTime = r.MorningTime != null ? r.MorningTime.Value.ToString("HH:mm") : null,
                    EveningTime = r.EveningTime != null ? r.EveningTime.Value.ToString("HH:mm") : null,
                    StopCount = r.Stops.Count(s => s.IsActive),
                    IsActive = r.IsActive
                }).ToListAsync();
            return ApiResponse<PagedResult<RouteListDto>>.Ok(new PagedResult<RouteListDto> { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }

        public async Task<List<RouteListDto>> GetDropdownAsync(string? search = null)
        {
            var q = _db.Routes.Include(r => r.Stops).Where(r => r.IsActive).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(r => r.RouteName.Contains(search) || r.RouteCode.Contains(search));

            return await q.OrderBy(r => r.RouteName).Take(100)
                .Select(r => new RouteListDto
                {
                    RouteId = r.RouteId,
                    RouteName = r.RouteName,
                    RouteCode = r.RouteCode,
                    StartStopName = r.Stops.Where(s => s.IsActive).OrderBy(s => s.StopOrder).Select(s => s.StopName).FirstOrDefault(),
                    EndStopName = r.Stops.Where(s => s.IsActive).OrderBy(s => s.StopOrder).Select(s => s.StopName).LastOrDefault(),
                    MorningTime = r.MorningTime != null ? r.MorningTime.Value.ToString("HH:mm") : null,
                    EveningTime = r.EveningTime != null ? r.EveningTime.Value.ToString("HH:mm") : null,
                    StopCount = r.Stops.Count(s => s.IsActive),
                    IsActive = r.IsActive
                }).ToListAsync();
        }

        public Task<int> GetListPageSizeAsync() => PaginationHelper.GetListPageSizeAsync(_db);
        public async Task<ApiResponse<RouteDetailDto>> GetByIdAsync(int routeId)
        {
            var r = await _db.Routes.Include(x => x.Stops.Where(s => s.IsActive)).FirstOrDefaultAsync(x => x.RouteId == routeId && x.IsActive);
            if (r is null) return ApiResponse<RouteDetailDto>.Fail("Not found.");
            var activeStops = r.Stops.Where(s => s.IsActive).OrderBy(s => s.StopOrder).ToList();
            return ApiResponse<RouteDetailDto>.Ok(new RouteDetailDto
            {
                RouteId = r.RouteId,
                RouteName = r.RouteName,
                RouteCode = r.RouteCode,
                Description = r.Description,
                StartStopName = activeStops.FirstOrDefault()?.StopName,
                EndStopName = activeStops.LastOrDefault()?.StopName,
                MorningTime = r.MorningTime?.ToString("HH:mm"),
                EveningTime = r.EveningTime?.ToString("HH:mm"),
                StopCount = activeStops.Count,
                IsActive = r.IsActive,
                Stops = activeStops.Select(s => new StopDto
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
        {
            var maxOrder = await _db.Stops.Where(s => s.RouteId == dto.RouteId && s.IsActive).Select(s => (int?)s.StopOrder).MaxAsync() ?? 0;
            _db.Stops.Add(new Stop { RouteId = dto.RouteId, StopName = dto.StopName, StopOrder = maxOrder + 1, Latitude = dto.Latitude, Longitude = dto.Longitude, MorningTime = dto.MorningTime is not null ? TimeOnly.Parse(dto.MorningTime) : null, EveningTime = dto.EveningTime is not null ? TimeOnly.Parse(dto.EveningTime) : null });
            await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Stop added.");
        }
        public async Task<ApiResponse<bool>> DeleteStopAsync(int stopId)
        { var s = await _db.Stops.FindAsync(stopId); if (s is null) return ApiResponse<bool>.Fail("Not found."); _db.Stops.Remove(s); await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Removed."); }
        public async Task<ApiResponse<bool>> ReorderStopsAsync(ReorderStopsDto dto)
        {
            if (dto.Stops is null || dto.Stops.Count == 0) return ApiResponse<bool>.Fail("No order changes received.");
            if (dto.Stops.Select(x => x.StopOrder).Distinct().Count() != dto.Stops.Count)
                return ApiResponse<bool>.Fail("Order numbers must be unique.");

            var stopIds = dto.Stops.Select(x => x.StopId).ToList();
            var stops = await _db.Stops.Where(s => s.RouteId == dto.RouteId && s.IsActive && stopIds.Contains(s.StopId)).ToListAsync();
            if (stops.Count != dto.Stops.Count) return ApiResponse<bool>.Fail("Not found.");

            // The retrying execution strategy (EnableRetryOnFailure) must own the whole
            // transaction so it can retry the entire unit as one block if a transient error occurs.
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync();

                // Phase 1: move every affected stop to a temporary negative order so the
                // (RouteId, StopOrder) unique index never sees a collision mid-update
                // (a straight swap like 1<->2 would otherwise be a circular dependency EF can't order safely).
                foreach (var stop in stops)
                    stop.StopOrder = -stop.StopId;
                await _db.SaveChangesAsync();

                // Phase 2: apply the real, final order values.
                foreach (var item in dto.Stops)
                {
                    var stop = stops.First(s => s.StopId == item.StopId);
                    stop.StopOrder = item.StopOrder;
                }
                await _db.SaveChangesAsync();

                await tx.CommitAsync();
            });

            return ApiResponse<bool>.Ok(true, "Stop order updated.");
        }
        public async Task<ApiResponse<bool>> UpdateStopsAsync(BusTracking.Common.DTOs.Stop.UpdateStopsDto dto)
        {
            if (dto.Stops is null || dto.Stops.Count == 0) return ApiResponse<bool>.Fail("No stop records received.");
            if (dto.Stops.Select(x => x.StopOrder).Distinct().Count() != dto.Stops.Count)
                return ApiResponse<bool>.Fail("Order numbers must be unique.");
            if (dto.Stops.Any(x => string.IsNullOrWhiteSpace(x.StopName)))
                return ApiResponse<bool>.Fail("Stop name cannot be empty.");

            var stopIds = dto.Stops.Select(x => x.StopId).ToList();
            var stops = await _db.Stops.Where(s => s.RouteId == dto.RouteId && s.IsActive && stopIds.Contains(s.StopId)).ToListAsync();
            if (stops.Count != dto.Stops.Count) return ApiResponse<bool>.Fail("Not all stops were found.");

            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync();

                // Phase 1: move to negative orders and update details
                foreach (var stop in stops)
                {
                    var item = dto.Stops.First(s => s.StopId == stop.StopId);
                    stop.StopName = item.StopName;
                    stop.MorningTime = item.MorningTime is not null ? TimeOnly.Parse(item.MorningTime) : null;
                    stop.EveningTime = item.EveningTime is not null ? TimeOnly.Parse(item.EveningTime) : null;
                    stop.Latitude = item.Latitude;
                    stop.Longitude = item.Longitude;
                    stop.StopOrder = -stop.StopId; // prevent unique index collision
                }
                await _db.SaveChangesAsync();

                // Phase 2: set the correct orders
                foreach (var item in dto.Stops)
                {
                    var stop = stops.First(s => s.StopId == item.StopId);
                    stop.StopOrder = item.StopOrder;
                }
                await _db.SaveChangesAsync();

                await tx.CommitAsync();
            });

            return ApiResponse<bool>.Ok(true, "Stops updated successfully.");
        }
        public async Task<ApiResponse<List<StopDto>>> GetStopsByRouteAsync(int routeId)
        {
            var stops = await _db.Stops.Where(s => s.RouteId == routeId && s.IsActive)
                .OrderBy(s => s.StopOrder)
                .Select(s => new StopDto { StopId = s.StopId, StopName = s.StopName, StopOrder = s.StopOrder, Latitude = s.Latitude, Longitude = s.Longitude, MorningTime = s.MorningTime != null ? s.MorningTime.Value.ToString("HH:mm") : null, EveningTime = s.EveningTime != null ? s.EveningTime.Value.ToString("HH:mm") : null })
                .ToListAsync();
            return ApiResponse<List<StopDto>>.Ok(stops);
        }
        public async Task<ApiResponse<bool>> ToggleActiveAsync(int routeId)
        {
            var r = await _db.Routes.FindAsync(routeId);
            if (r is null) return ApiResponse<bool>.Fail("Not found.");
            r.IsActive = !r.IsActive; r.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, r.IsActive ? "Route activated." : "Route deactivated.");
        }

        public async Task<ApiResponse<List<StopDto>>> GetStopsByBusAsync(int busId)
        {
            var bus = await _db.Buses.Include(b => b.Route).FirstOrDefaultAsync(b => b.BusId == busId);
            if (bus?.RouteId == null) return ApiResponse<List<StopDto>>.Ok([]);
            return await GetStopsByRouteAsync(bus.RouteId.Value);
        }
    }
}
