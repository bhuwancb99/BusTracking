namespace BusTracking.Common.Services
{
    public class BusService : IBusService
    {
        private readonly AppDbContext _db;
        public BusService(AppDbContext db) => _db = db;

        public async Task<ApiResponse<PagedResult<BusListDto>>> GetAllAsync(int page, string? search, string? status)
        {
            var q = _db.Buses
                .Include(b => b.Route)
                .Include(b => b.BusType)
                .Include(b => b.Driver).ThenInclude(d => d!.User)
                .Include(b => b.Students)
                .AsQueryable();
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
                    RouteId = b.RouteId,
                    RouteName = b.Route != null ? b.Route.RouteName : null,
                    BusTypeId = b.BusTypeId,
                    BusTypeName = b.BusType != null ? b.BusType.Name : null,
                    DriverUserId = b.Driver != null ? b.Driver.UserId : (int?)null,
                    DriverName = b.Driver != null ? b.Driver.User.FullName : null,
                    DriverPhone = b.Driver != null ? b.Driver.User.PhoneNumber : null,
                    Capacity = b.Capacity,
                    StudentCount = b.Students.Count,
                    IsActive = b.IsActive
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
                .Include(x => x.Route)
                .Include(x => x.BusType)
                .Include(x => x.Driver).ThenInclude(d => d!.User)
                .Include(x => x.Students)
                .Include(x => x.Images.OrderBy(i => i.DisplayOrder))
                .FirstOrDefaultAsync(x => x.BusId == busId);

            if (b is null)
                return ApiResponse<BusListDto>.Fail("Not found.");

            return ApiResponse<BusListDto>.Ok(new BusListDto
            {
                BusId = b.BusId,
                BusName = b.BusName,
                BusNumber = b.BusNumber,
                RouteId = b.RouteId,
                RouteName = b.Route?.RouteName,
                BusTypeId = b.BusTypeId,
                BusTypeName = b.BusType?.Name,
                DriverUserId = b.Driver?.UserId,
                DriverName = b.Driver?.User.FullName,
                DriverPhone = b.Driver?.User.PhoneNumber,
                Capacity = b.Capacity,
                StudentCount = b.Students.Count,
                IsActive = b.IsActive,
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

            var bus = new Bus
            {
                BusName = dto.BusName,
                BusNumber = dto.BusNumber,
                RouteId = dto.RouteId,
                BusTypeId = dto.BusTypeId,
                Capacity = dto.Capacity,
                CreatedBy = createdBy
            };
            _db.Buses.Add(bus); await _db.SaveChangesAsync();
            if (dto.DriverUserId.HasValue)
            {
                var d = await _db.DriverDetails.FirstOrDefaultAsync(x => x.UserId == dto.DriverUserId.Value);
                if (d is not null) { d.BusId = bus.BusId; d.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
            }
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

            bus.BusName = dto.BusName;
            bus.BusNumber = dto.BusNumber;
            bus.RouteId = dto.RouteId;
            bus.BusTypeId = dto.BusTypeId;
            bus.Capacity = dto.Capacity;
            bus.IsActive = dto.IsActive;
            bus.UpdatedAt = DateTime.UtcNow;
            // Handle driver assignment change
            if (dto.DriverUserId.HasValue)
            {
                // Unlink previous driver of this bus
                var prev = await _db.DriverDetails.FirstOrDefaultAsync(d => d.BusId == busId && d.UserId != dto.DriverUserId.Value);
                if (prev is not null) { prev.BusId = null; prev.UpdatedAt = DateTime.UtcNow; }
                var d = await _db.DriverDetails.FirstOrDefaultAsync(x => x.UserId == dto.DriverUserId.Value);
                if (d is not null) { d.BusId = busId; d.UpdatedAt = DateTime.UtcNow; }
            }
            else
            {
                // Remove any driver from this bus
                var prev = await _db.DriverDetails.FirstOrDefaultAsync(d => d.BusId == busId);
                if (prev is not null) { prev.BusId = null; prev.UpdatedAt = DateTime.UtcNow; }
            }
            await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Bus updated.");
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
            var prev = await _db.DriverDetails.FirstOrDefaultAsync(d => d.BusId == dto.BusId);
            if (prev is not null) { prev.BusId = null; prev.UpdatedAt = DateTime.UtcNow; }
            if (dto.DriverUserId.HasValue)
            { var d = await _db.DriverDetails.FirstOrDefaultAsync(x => x.UserId == dto.DriverUserId.Value); if (d is not null) { d.BusId = dto.BusId; d.UpdatedAt = DateTime.UtcNow; } }
            await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Driver assigned.");
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
            var q = _db.Buses.Where(b => b.IsActive);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(b => b.BusName.Contains(search) || b.BusNumber.Contains(search));
            var list = await q.OrderBy(b => b.BusName).Take(20).Select(b => new BusDropdownDto { BusId = b.BusId, Display = $"{b.BusName} ({b.BusNumber})" }).ToListAsync();
            return ApiResponse<List<BusDropdownDto>>.Ok(list);
        }
    }
}
