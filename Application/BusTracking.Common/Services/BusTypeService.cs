namespace BusTracking.Common.Services
{
    public class BusTypeService : IBusTypeService
    {
        private readonly AppDbContext _db;
        public BusTypeService(AppDbContext db) => _db = db;

        public async Task<ApiResponse<PagedResult<BusTypeDto>>> GetAllAsync(string? search = null, int page = 1)
        {
            var q = _db.BusTypeMasters.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(t => t.Name.Contains(search));

            var pageSize = await GetListPageSizeAsync();
            page = PaginationHelper.Clamp(page);

            var total = await q.CountAsync();
            var items = await q.OrderBy(t => t.Name)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(t => new BusTypeDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    BusCount = t.Buses.Count,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToListAsync();

            return ApiResponse<PagedResult<BusTypeDto>>.Ok(new PagedResult<BusTypeDto>
            {
                Items = items,
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize
            });
        }

        public async Task<int> GetListPageSizeAsync()
        {
            var raw = await _db.AppConfigurations
                .Where(c => c.ConfigKey == AppConstants.AppConfigPageSizeKey && c.IsActive)
                .Select(c => c.ConfigValue)
                .FirstOrDefaultAsync();

            return int.TryParse(raw, out var size) && size > 0
                ? PaginationHelper.ClampPageSize(size)
                : AppConstants.DefaultPageSize;
        }

        public async Task<ApiResponse<BusTypeDto>> GetByIdAsync(int id)
        {
            var t = await _db.BusTypeMasters.FindAsync(id);
            if (t is null)
                return ApiResponse<BusTypeDto>.Fail("Bus type not found.");

            return ApiResponse<BusTypeDto>.Ok(new BusTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                BusCount = await _db.Buses.CountAsync(b => b.BusTypeId == t.Id),
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            });
        }

        public async Task<ApiResponse<BusTypeDto>> CreateAsync(SaveBusTypeDto dto)
        {
            var name = dto.Name.Trim();
            if (await _db.BusTypeMasters.AnyAsync(t => t.Name == name))
                return ApiResponse<BusTypeDto>.Fail($"Bus type '{name}' already exists.");

            var entity = new BusTypeMaster
            {
                Name = name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.BusTypeMasters.Add(entity);
            await _db.SaveChangesAsync();

            return ApiResponse<BusTypeDto>.Ok(new BusTypeDto
            {
                Id = entity.Id,
                Name = entity.Name,
                BusCount = 0,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            }, "Bus type added successfully.");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, SaveBusTypeDto dto)
        {
            var t = await _db.BusTypeMasters.FindAsync(id);
            if (t is null)
                return ApiResponse<bool>.Fail("Bus type not found.");

            var name = dto.Name.Trim();
            if (await _db.BusTypeMasters.AnyAsync(x => x.Name == name && x.Id != id))
                return ApiResponse<bool>.Fail($"Bus type '{name}' already exists.");

            t.Name = name;
            t.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Bus type updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var t = await _db.BusTypeMasters.FindAsync(id);
            if (t is null)
                return ApiResponse<bool>.Fail("Bus type not found.");

            var inUse = await _db.Buses.AnyAsync(b => b.BusTypeId == id);
            if (inUse)
                return ApiResponse<bool>.Fail("Cannot delete — this bus type is assigned to one or more buses.");

            _db.BusTypeMasters.Remove(t);
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Bus type deleted.");
        }

        public async Task<ApiResponse<List<BusTypeDropdownDto>>> GetDropdownAsync()
        {
            var list = await _db.BusTypeMasters
                .OrderBy(t => t.Name)
                .Select(t => new BusTypeDropdownDto { Id = t.Id, Name = t.Name })
                .ToListAsync();

            return ApiResponse<List<BusTypeDropdownDto>>.Ok(list);
        }
    }
}
