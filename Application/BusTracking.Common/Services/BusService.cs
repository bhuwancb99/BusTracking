namespace BusTracking.Common.Services
{
    public class BusService : IBusService
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public BusService(AppDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<PagedResult<BusListDto>>> GetAllAsync(int page, string? search, string? status)
        {
            var schoolId = _currentUser.SchoolId;
            var q = _db.Buses.IgnoreQueryFilters()
                .Include(b => b.BusType)
                .Include(b => b.Students)
                .Include(b => b.RouteMappings).ThenInclude(rm => rm.Route)
                .Include(b => b.DriverMappings).ThenInclude(dm => dm.DriverUser)
                .AsQueryable();

            if (schoolId.HasValue)
            {
                q = q.Where(b => b.SchoolId == schoolId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(b => b.BusName.Contains(search) || b.BusNumber.Contains(search));
            if (status == "Active") q = q.Where(b => b.IsActive);
            else if (status == "Inactive") q = q.Where(b => !b.IsActive);

            var pageSize = await GetListPageSizeAsync();
            page = PaginationHelper.Clamp(page);

            var total = await q.CountAsync();
            var items = await q.OrderBy(b => b.BusName).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(b => new BusListDto
                {
                    BusId = b.BusId,
                    BusName = b.BusName,
                    BusNumber = b.BusNumber,
                    RouteName = b.RouteMappings.Any() ? string.Join(", ", b.RouteMappings.Select(rm => rm.Route.RouteName)) : "—",
                    RouteIds = b.RouteMappings.Select(rm => rm.RouteId).ToList(),
                    RouteNames = b.RouteMappings.Select(rm => rm.Route.RouteName).ToList(),
                    BusTypeId = b.BusTypeId,
                    BusTypeName = b.BusType != null ? b.BusType.Name : null,
                    DriverName = b.DriverMappings.Any() ? string.Join(", ", b.DriverMappings.Select(dm => dm.DriverUser.FullName)) : "—",
                    DriverUserIds = b.DriverMappings.Select(dm => dm.DriverUserId).ToList(),
                    DriverNames = b.DriverMappings.Select(dm => dm.DriverUser.FullName).ToList(),
                    Capacity = b.Capacity,
                    StudentCount = b.Students.Count,
                    IsActive = b.IsActive,
                    InsuranceExpiryDate = b.InsuranceExpiryDate,
                    FitnessExpiryDate = b.FitnessExpiryDate,
                    PucExpiryDate = b.PucExpiryDate,
                    LastServiceDate = b.LastServiceDate
                }).ToListAsync();

            return ApiResponse<PagedResult<BusListDto>>.Ok(new PagedResult<BusListDto>
            {
                Items = items,
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize
            });
        }

        public Task<int> GetListPageSizeAsync() => PaginationHelper.GetListPageSizeAsync(_db);

        public async Task<ApiResponse<BusListDto>> GetByIdAsync(int busId)
        {
            var b = await _db.Buses
                .Include(x => x.BusType)
                .Include(x => x.Students)
                .Include(x => x.Images.OrderBy(i => i.DisplayOrder))
                .Include(x => x.RouteMappings).ThenInclude(rm => rm.Route)
                .Include(x => x.DriverMappings).ThenInclude(dm => dm.DriverUser)
                .FirstOrDefaultAsync(x => x.BusId == busId);

            if (b is null)
                return ApiResponse<BusListDto>.Fail("Not found.");

            return ApiResponse<BusListDto>.Ok(new BusListDto
            {
                BusId = b.BusId,
                BusName = b.BusName,
                BusNumber = b.BusNumber,
                RouteName = b.RouteMappings.Any() ? string.Join(", ", b.RouteMappings.Select(rm => rm.Route.RouteName)) : "—",
                RouteIds = b.RouteMappings.Select(rm => rm.RouteId).ToList(),
                RouteNames = b.RouteMappings.Select(rm => rm.Route.RouteName).ToList(),
                BusTypeId = b.BusTypeId,
                BusTypeName = b.BusType?.Name,
                DriverName = b.DriverMappings.Any() ? string.Join(", ", b.DriverMappings.Select(dm => dm.DriverUser.FullName)) : "—",
                DriverUserIds = b.DriverMappings.Select(dm => dm.DriverUserId).ToList(),
                DriverNames = b.DriverMappings.Select(dm => dm.DriverUser.FullName).ToList(),
                Capacity = b.Capacity,
                StudentCount = b.Students.Count,
                IsActive = b.IsActive,
                InsuranceExpiryDate = b.InsuranceExpiryDate,
                FitnessExpiryDate = b.FitnessExpiryDate,
                PucExpiryDate = b.PucExpiryDate,
                LastServiceDate = b.LastServiceDate,
                PrimaryImageUrl = b.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                                  ?? b.Images.FirstOrDefault()?.ImageUrl,
                Images = b.Images.Select(i => new BusImageDto
                {
                    BusImageId = i.BusImageId,
                    ImageUrl = i.ImageUrl,
                    DisplayOrder = i.DisplayOrder,
                    IsPrimary = i.IsPrimary
                }).ToList()
            });
        }

        public async Task<ApiResponse<bool>> CreateAsync(CreateBusDto dto, int createdBy)
        {
            if (await _db.Buses.AnyAsync(b => b.BusNumber == dto.BusNumber))
                return ApiResponse<bool>.Fail("Bus number exists.");

            if (!await _db.BusTypeMasters.AnyAsync(t => t.Id == dto.BusTypeId))
                return ApiResponse<bool>.Fail("Selected bus type is invalid.");

            var routeIds = dto.RouteIds?.Distinct().ToList() ?? [];
            var driverUserIds = dto.DriverUserIds?.Distinct().ToList() ?? [];

            var bus = new Bus
            {
                SchoolId = _currentUser.SchoolId,
                BusName = dto.BusName,
                BusNumber = dto.BusNumber,
                BusTypeId = dto.BusTypeId,
                Capacity = dto.Capacity,
                InsuranceExpiryDate = dto.InsuranceExpiryDate,
                FitnessExpiryDate = dto.FitnessExpiryDate,
                PucExpiryDate = dto.PucExpiryDate,
                LastServiceDate = dto.LastServiceDate,
                CreatedBy = createdBy
            };
            _db.Buses.Add(bus);
            await _db.SaveChangesAsync();

            // Save multi-routes
            foreach (var rId in routeIds)
            {
                _db.BusRouteMappings.Add(new BusRouteMapping { BusId = bus.BusId, RouteId = rId });
            }

            // Save multi-drivers
            foreach (var dUserId in driverUserIds)
            {
                _db.BusDriverMappings.Add(new BusDriverMapping { BusId = bus.BusId, DriverUserId = dUserId });
            }

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Bus created.");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int busId, UpdateBusDto dto)
        {
            var bus = await _db.Buses.FindAsync(busId);
            if (bus is null)
                return ApiResponse<bool>.Fail("Not found.");
            if (await _db.Buses.AnyAsync(b => b.BusNumber == dto.BusNumber && b.BusId != busId))
                return ApiResponse<bool>.Fail("Bus number in use.");

            if (!await _db.BusTypeMasters.AnyAsync(t => t.Id == dto.BusTypeId))
                return ApiResponse<bool>.Fail("Selected bus type is invalid.");

            var routeIds = dto.RouteIds?.Distinct().ToList() ?? [];
            var driverUserIds = dto.DriverUserIds?.Distinct().ToList() ?? [];

            bus.BusName = dto.BusName;
            bus.BusNumber = dto.BusNumber;
            bus.BusTypeId = dto.BusTypeId;
            bus.Capacity = dto.Capacity;
            bus.InsuranceExpiryDate = dto.InsuranceExpiryDate;
            bus.FitnessExpiryDate = dto.FitnessExpiryDate;
            bus.PucExpiryDate = dto.PucExpiryDate;
            bus.LastServiceDate = dto.LastServiceDate;
            bus.IsActive = dto.IsActive;
            bus.UpdatedAt = DateTime.UtcNow;

            // Update route mappings
            var oldRoutes = await _db.BusRouteMappings.Where(rm => rm.BusId == busId).ToListAsync();
            _db.BusRouteMappings.RemoveRange(oldRoutes);
            foreach (var rId in routeIds)
            {
                _db.BusRouteMappings.Add(new BusRouteMapping { BusId = busId, RouteId = rId });
            }

            // Update driver mappings
            var oldDrivers = await _db.BusDriverMappings.Where(dm => dm.BusId == busId).ToListAsync();
            _db.BusDriverMappings.RemoveRange(oldDrivers);
            foreach (var dUserId in driverUserIds)
            {
                _db.BusDriverMappings.Add(new BusDriverMapping { BusId = busId, DriverUserId = dUserId });
            }

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Bus updated.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int busId)
        {
            var b = await _db.Buses.FindAsync(busId);
            if (b is null)
                return ApiResponse<bool>.Fail("Not found.");
            b.IsActive = false;
            b.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Marked inactive.");
        }

        public async Task<ApiResponse<bool>> ToggleActiveAsync(int busId)
        {
            var b = await _db.Buses.FindAsync(busId);
            if (b is null)
                return ApiResponse<bool>.Fail("Not found.");
            b.IsActive = !b.IsActive;
            b.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, b.IsActive ? "Activated." : "Deactivated.");
        }

        public async Task<ApiResponse<bool>> AssignDriverAsync(AssignDriverToBusDto dto)
        {
            if (dto.DriverUserId.HasValue)
            {
                if (!await _db.BusDriverMappings.AnyAsync(dm => dm.BusId == dto.BusId && dm.DriverUserId == dto.DriverUserId.Value))
                {
                    _db.BusDriverMappings.Add(new BusDriverMapping { BusId = dto.BusId, DriverUserId = dto.DriverUserId.Value });
                    await _db.SaveChangesAsync();
                }
            }
            return ApiResponse<bool>.Ok(true, "Driver assigned.");
        }

        public async Task<ApiResponse<bool>> AssignStudentAsync(int busId, int studentId)
        {
            var s = await _db.Students.FindAsync(studentId);
            if (s is null)
                return ApiResponse<bool>.Fail("Student not found.");
            s.BusId = busId;
            s.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Assigned.");
        }

        public async Task<ApiResponse<bool>> RemoveStudentAsync(int busId, int studentId)
        {
            var s = await _db.Students.FindAsync(studentId);
            if (s is null)
                return ApiResponse<bool>.Fail("Not found.");
            s.BusId = null;
            s.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Removed.");
        }

        public async Task<ApiResponse<List<BusDropdownDto>>> GetDropdownAsync(string? search)
        {
            var schoolId = _currentUser.SchoolId;
            var q = _db.Buses.IgnoreQueryFilters().Where(b => b.IsActive);
            if (schoolId.HasValue) q = q.Where(b => b.SchoolId == schoolId.Value);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(b => b.BusName.Contains(search) || b.BusNumber.Contains(search));
            var list = await q.OrderBy(b => b.BusName).Select(b => new BusDropdownDto { BusId = b.BusId, Display = $"{b.BusName} ({b.BusNumber})" }).ToListAsync();
            return ApiResponse<List<BusDropdownDto>>.Ok(list);
        }

        public async Task<ApiResponse<List<RouteListDto>>> GetRoutesForBusAsync(int busId)
        {
            var mappedRouteIds = await _db.BusRouteMappings.Where(rm => rm.BusId == busId).Select(rm => rm.RouteId).ToListAsync();

            var q = _db.Routes.Where(r => r.IsActive);
            if (mappedRouteIds.Any())
            {
                q = q.Where(r => mappedRouteIds.Contains(r.RouteId));
            }

            var list = await q.OrderBy(r => r.RouteName).Select(r => new RouteListDto
            {
                RouteId = r.RouteId,
                RouteName = r.RouteName,
                RouteCode = r.RouteCode
            }).ToListAsync();

            return ApiResponse<List<RouteListDto>>.Ok(list);
        }

        public async Task<ApiResponse<List<DriverDropdownDto>>> GetDriversForBusAsync(int busId)
        {
            var schoolId = _currentUser.SchoolId;
            var mappedDriverUserIds = await _db.BusDriverMappings.Where(dm => dm.BusId == busId).Select(dm => dm.DriverUserId).ToListAsync();

            var roleId = await _db.Roles.Where(r => r.RoleName == "Driver").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users.IgnoreQueryFilters().Where(u => u.RoleId == roleId && u.IsActive);
            if (schoolId.HasValue) q = q.Where(u => u.SchoolId == schoolId.Value || (u.DriverDetail != null && u.DriverDetail.SchoolId == schoolId.Value));

            var allActiveDrivers = await q.OrderBy(u => u.FullName).Select(u => new DriverDropdownDto
            {
                UserId = u.UserId,
                Display = u.FullName + " (" + u.UserName + ")"
            }).ToListAsync();

            // Order drivers so assigned ones come first
            if (mappedDriverUserIds.Any())
            {
                allActiveDrivers = allActiveDrivers
                    .OrderByDescending(d => mappedDriverUserIds.Contains(d.UserId))
                    .ThenBy(d => d.Display)
                    .ToList();
            }

            return ApiResponse<List<DriverDropdownDto>>.Ok(allActiveDrivers);
        }
    }
}
