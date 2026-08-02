namespace BusTracking.Common.Services
{
    public class GlobalConfigService : IGlobalConfigService
    {
        private readonly AppDbContext _db;

        public GlobalConfigService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<GlobalConfigDto>> GetAllAsync(string? search = null, bool? isActive = null, int page = 1, int pageSize = 20)
        {
            var query = _db.GlobalConfigurations.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(c => EF.Functions.Like(c.GlobalConfigKey, $"%{s}%") ||
                                         (c.Description != null && EF.Functions.Like(c.Description, $"%{s}%")));
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query.OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => MapToDto(c))
                .ToListAsync();

            return new PagedResult<GlobalConfigDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<GlobalConfigDto>> GetByIdAsync(int id)
        {
            var config = await _db.GlobalConfigurations.AsNoTracking().FirstOrDefaultAsync(c => c.GlobalConfigId == id);
            if (config is null) return ApiResponse<GlobalConfigDto>.Fail("Global configuration not found.");
            return ApiResponse<GlobalConfigDto>.Ok(MapToDto(config));
        }

        public async Task<ApiResponse<GlobalConfigDto>> GetByKeyAsync(string key)
        {
            var config = await _db.GlobalConfigurations.AsNoTracking()
                .FirstOrDefaultAsync(c => c.GlobalConfigKey.ToLower() == key.ToLower() && c.IsActive);
            if (config is null) return ApiResponse<GlobalConfigDto>.Fail("Global configuration key not found or inactive.");
            return ApiResponse<GlobalConfigDto>.Ok(MapToDto(config));
        }

        public async Task<ApiResponse<GlobalConfigDto>> CreateAsync(CreateGlobalConfigDto dto)
        {
            var key = dto.GlobalConfigKey.Trim();
            var exists = await _db.GlobalConfigurations.AnyAsync(c => c.GlobalConfigKey.ToLower() == key.ToLower());
            if (exists) return ApiResponse<GlobalConfigDto>.Fail($"Global configuration key '{key}' already exists.");

            var entity = new GlobalConfiguration
            {
                GlobalConfigKey = key,
                GlobalConfigValue = dto.GlobalConfigValue.Trim(),
                Description = dto.Description?.Trim(),
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.GlobalConfigurations.Add(entity);
            await _db.SaveChangesAsync();

            return ApiResponse<GlobalConfigDto>.Ok(MapToDto(entity), "Global configuration created successfully.");
        }

        public async Task<ApiResponse<GlobalConfigDto>> UpdateAsync(int id, UpdateGlobalConfigDto dto)
        {
            var config = await _db.GlobalConfigurations.FirstOrDefaultAsync(c => c.GlobalConfigId == id);
            if (config is null) return ApiResponse<GlobalConfigDto>.Fail("Global configuration not found.");

            config.GlobalConfigValue = dto.GlobalConfigValue.Trim();
            config.Description = dto.Description?.Trim();
            config.IsActive = dto.IsActive;
            config.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return ApiResponse<GlobalConfigDto>.Ok(MapToDto(config), "Global configuration updated successfully.");
        }

        public async Task<ApiResponse<bool>> ToggleActiveAsync(int id)
        {
            var config = await _db.GlobalConfigurations.FirstOrDefaultAsync(c => c.GlobalConfigId == id);
            if (config is null) return ApiResponse<bool>.Fail("Global configuration not found.");

            config.IsActive = !config.IsActive;
            config.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var status = config.IsActive ? "activated" : "deactivated";
            return ApiResponse<bool>.Ok(config.IsActive, $"Global configuration {status} successfully.");
        }

        private static GlobalConfigDto MapToDto(GlobalConfiguration c) => new()
        {
            GlobalConfigId = c.GlobalConfigId,
            GlobalConfigKey = c.GlobalConfigKey,
            GlobalConfigValue = c.GlobalConfigValue,
            Description = c.Description,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }
}
