namespace BusTracking.Common.Services
{
    public class AppConfigService : IAppConfigService
    {
        private readonly AppDbContext _db;
        public AppConfigService(AppDbContext db) => _db = db;

        public async Task<ApiResponse<PagedResult<AppConfigDto>>> GetAllAsync(
            string? platform, string? search, bool? isActive, int page = 1)
        {
            var q = _db.AppConfigurations
                .Include(c => c.CreatedByUser)
                .AsQueryable();

            // Exact platform match only — selecting "Web" shows Web-only rows,
            // "Mobile" shows Mobile-only rows, "Both" shows Web/Mobile rows.
            // (No implicit merge with "Both" when a single platform is selected.)
            if (!string.IsNullOrWhiteSpace(platform) &&
                Enum.TryParse<ConfigPlatform>(platform, true, out var p))
                q = q.Where(c => c.Platform == p);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(c => c.ConfigKey.Contains(search) || c.ConfigValue.Contains(search));

            if (isActive.HasValue)
                q = q.Where(c => c.IsActive == isActive.Value);

            var pageSize = await GetListPageSizeAsync();
            page = PaginationHelper.Clamp(page);

            var total = await q.CountAsync();
            var items = await q.OrderBy(c => c.Platform).ThenBy(c => c.ConfigKey)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(c => new AppConfigDto
                {
                    ConfigId = c.ConfigId,
                    ConfigKey = c.ConfigKey,
                    ConfigValue = c.ConfigValue,
                    Description = c.Description,
                    Platform = c.Platform.ToString(),
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    CreatedByName = c.CreatedByUser.FullName
                }).ToListAsync();

            return ApiResponse<PagedResult<AppConfigDto>>.Ok(new PagedResult<AppConfigDto>
            {
                Items = items,
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize
            });
        }

        public async Task<string?> GetValueAsync(string configKey)
        {
            if (string.IsNullOrWhiteSpace(configKey)) return null;
            return await _db.AppConfigurations
                .Where(c => c.ConfigKey == configKey && c.IsActive)
                .Select(c => c.ConfigValue)
                .FirstOrDefaultAsync();
        }

        public async Task<T> GetValueAsync<T>(string configKey, T defaultValue)
        {
            var raw = await GetValueAsync(configKey);
            if (string.IsNullOrWhiteSpace(raw)) return defaultValue;
            try
            {
                return (T)Convert.ChangeType(raw, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public Task<int> GetListPageSizeAsync() => PaginationHelper.GetListPageSizeAsync(_db);

        public async Task<ApiResponse<AppConfigDto>> GetByIdAsync(int configId)
        {
            var c = await _db.AppConfigurations
                .Include(x => x.CreatedByUser)
                .FirstOrDefaultAsync(x => x.ConfigId == configId);

            if (c is null)
                return ApiResponse<AppConfigDto>.Fail("Configuration not found.");

            return ApiResponse<AppConfigDto>.Ok(new AppConfigDto
            {
                ConfigId = c.ConfigId,
                ConfigKey = c.ConfigKey,
                ConfigValue = c.ConfigValue,
                Description = c.Description,
                Platform = c.Platform.ToString(),
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                CreatedByName = c.CreatedByUser.FullName
            });
        }

        public async Task<ApiResponse<bool>> CreateAsync(CreateAppConfigDto dto, int createdBy)
        {
            // Ensure unique key per platform
            var exists = await _db.AppConfigurations
                .AnyAsync(c => c.ConfigKey == dto.ConfigKey && c.Platform == dto.PlatformEnum);
            if (exists)
                return ApiResponse<bool>.Fail($"Key '{dto.ConfigKey}' already exists for platform '{dto.Platform}'.");

            _db.AppConfigurations.Add(new AppConfiguration
            {
                ConfigKey = dto.ConfigKey.Trim(),
                ConfigValue = dto.ConfigValue.Trim(),
                Description = dto.Description?.Trim(),
                Platform = dto.PlatformEnum,
                IsActive = dto.IsActive,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Configuration created successfully.");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int configId, UpdateAppConfigDto dto)
        {
            var c = await _db.AppConfigurations.FindAsync(configId);
            if (c is null)
                return ApiResponse<bool>.Fail("Configuration not found.");

            // Check uniqueness only if key or platform changed
            if (c.ConfigKey != dto.ConfigKey || c.Platform != dto.PlatformEnum)
            {
                var exists = await _db.AppConfigurations
                    .AnyAsync(x => x.ConfigKey == dto.ConfigKey
                                && x.Platform == dto.PlatformEnum
                                && x.ConfigId != configId);
                if (exists)
                    return ApiResponse<bool>.Fail($"Key '{dto.ConfigKey}' already exists for platform '{dto.Platform}'.");
            }

            c.ConfigKey = dto.ConfigKey.Trim();
            c.ConfigValue = dto.ConfigValue.Trim();
            c.Description = dto.Description?.Trim();
            c.Platform = dto.PlatformEnum;
            c.IsActive = dto.IsActive;
            c.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Configuration updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int configId)
        {
            var c = await _db.AppConfigurations.FindAsync(configId);
            if (c is null)
                return ApiResponse<bool>.Fail("Configuration not found.");

            _db.AppConfigurations.Remove(c);
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Configuration deleted.");
        }

        public async Task<ApiResponse<bool>> ToggleActiveAsync(int configId)
        {
            var c = await _db.AppConfigurations.FindAsync(configId);
            if (c is null) return ApiResponse<bool>.Fail("Not found.");

            c.IsActive = !c.IsActive;
            c.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, c.IsActive ? "Activated." : "Deactivated.");
        }

        public async Task<ApiResponse<List<AppConfigValueDto>>> GetConfigForPlatformAsync(ConfigPlatform platform)
        {
            var list = await _db.AppConfigurations
                .Where(c => c.IsActive && (c.Platform == platform || c.Platform == ConfigPlatform.Both))
                .OrderBy(c => c.ConfigKey)
                .Select(c => new AppConfigValueDto { Key = c.ConfigKey, Value = c.ConfigValue })
                .ToListAsync();

            return ApiResponse<List<AppConfigValueDto>>.Ok(list);
        }
    }
}