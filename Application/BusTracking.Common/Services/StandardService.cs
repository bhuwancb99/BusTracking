namespace BusTracking.Common.Services
{
    public class StandardService : IStandardService
    {
        private readonly AppDbContext _db;
        public StandardService(AppDbContext db) => _db = db;

        public async Task<ApiResponse<PagedResult<StandardDto>>> GetAllAsync(string? search, bool? isActive, int page = 1)
        {
            var q = _db.StandardMasters.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(s => s.StandardName.Contains(search));

            if (isActive.HasValue)
                q = q.Where(s => s.IsActive == isActive.Value);

            // Fetch page size from AppConfigurations (matching AppConfigService)
            var rawPageSize = await _db.AppConfigurations
                .Where(c => c.ConfigKey == AppConstants.AppConfigPageSizeKey && c.IsActive)
                .Select(c => c.ConfigValue)
                .FirstOrDefaultAsync();

            int pageSize = int.TryParse(rawPageSize, out var size) && size > 0
                ? PaginationHelper.ClampPageSize(size)
                : AppConstants.DefaultPageSize;

            page = PaginationHelper.Clamp(page);
            var total = await q.CountAsync();

            var items = await q.OrderBy(s => s.StandardId)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(s => new StandardDto
                {
                    StandardId = s.StandardId,
                    StandardName = s.StandardName,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt
                }).ToListAsync();

            return ApiResponse<PagedResult<StandardDto>>.Ok(new PagedResult<StandardDto>
            {
                Items = items,
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize
            });
        }

        public async Task<ApiResponse<StandardDto>> GetByIdAsync(int standardId)
        {
            var s = await _db.StandardMasters.FindAsync(standardId);
            if (s is null)
                return ApiResponse<StandardDto>.Fail("Class/Standard not found.");

            return ApiResponse<StandardDto>.Ok(new StandardDto
            {
                StandardId = s.StandardId,
                StandardName = s.StandardName,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            });
        }

        public async Task<ApiResponse<bool>> CreateAsync(CreateStandardDto dto)
        {
            var exists = await _db.StandardMasters.AnyAsync(s => s.StandardName == dto.StandardName.Trim());
            if (exists)
                return ApiResponse<bool>.Fail($"Class/Standard '{dto.StandardName}' already exists.");

            _db.StandardMasters.Add(new StandardMaster
            {
                StandardName = dto.StandardName.Trim(),
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Class/Standard created successfully.");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int standardId, UpdateStandardDto dto)
        {
            var s = await _db.StandardMasters.FindAsync(standardId);
            if (s is null)
                return ApiResponse<bool>.Fail("Class/Standard not found.");

            if (s.StandardName != dto.StandardName.Trim())
            {
                var exists = await _db.StandardMasters.AnyAsync(x => x.StandardName == dto.StandardName.Trim() && x.StandardId != standardId);
                if (exists)
                    return ApiResponse<bool>.Fail($"Class/Standard '{dto.StandardName}' already exists.");
            }

            s.StandardName = dto.StandardName.Trim();
            s.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Class/Standard updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int standardId)
        {
            var s = await _db.StandardMasters.FindAsync(standardId);
            if (s is null)
                return ApiResponse<bool>.Fail("Class/Standard not found.");

            // Check if standard is assigned to any student
            var inUse = await _db.Students.AnyAsync(st => st.StandardId == standardId);
            if (inUse)
                return ApiResponse<bool>.Fail("Cannot delete this Class/Standard because it is assigned to one or more students.");

            _db.StandardMasters.Remove(s);
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Class/Standard deleted successfully.");
        }

        public async Task<ApiResponse<bool>> ToggleActiveAsync(int standardId)
        {
            var s = await _db.StandardMasters.FindAsync(standardId);
            if (s is null)
                return ApiResponse<bool>.Fail("Class/Standard not found.");

            s.IsActive = !s.IsActive;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, s.IsActive ? "Activated." : "Deactivated.");
        }

        public async Task<ApiResponse<List<StandardDto>>> GetActiveStandardsAsync()
        {
            var list = await _db.StandardMasters
                .Where(s => s.IsActive)
                .OrderBy(s => s.StandardId)
                .Select(s => new StandardDto
                {
                    StandardId = s.StandardId,
                    StandardName = s.StandardName,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt
                }).ToListAsync();

            return ApiResponse<List<StandardDto>>.Ok(list);
        }
    }
}
