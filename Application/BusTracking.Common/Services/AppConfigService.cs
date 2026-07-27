namespace BusTracking.Common.Services
{
    public class AppConfigService : IAppConfigService
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUserService _currentUserService;

        public AppConfigService(AppDbContext db, ICurrentUserService currentUserService)
        {
            _db = db;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<PagedResult<AppConfigDto>>> GetAllAsync(
            string? platform, string? search, bool? isActive, int page = 1)
        {
            var schoolId = _currentUserService.SchoolId;
            var q = _db.AppConfigurations.AsQueryable();

            if (schoolId.HasValue)
            {
                q = q.Where(c => c.SchoolId == schoolId.Value);
            }

            if (!string.IsNullOrWhiteSpace(platform) &&
                !platform.Equals("All", StringComparison.OrdinalIgnoreCase) &&
                !platform.Equals("Both", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<ConfigPlatform>(platform, true, out var p))
                {
                    q = q.Where(c => c.Platform == p || c.Platform == ConfigPlatform.Both);
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(c => c.ConfigKey.Contains(search) || c.ConfigValue.Contains(search));

            if (isActive.HasValue)
                q = q.Where(c => c.IsActive == isActive.Value);

            var pageSize = await GetListPageSizeAsync();
            page = PaginationHelper.Clamp(page);

            var total = await q.CountAsync();
            var rawItems = await q.OrderBy(c => c.Platform).ThenBy(c => c.ConfigKey)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(c => new
                {
                    c.ConfigId,
                    c.ConfigKey,
                    c.ConfigValue,
                    c.Description,
                    Platform = c.Platform.ToString(),
                    c.IsActive,
                    c.CreatedAt,
                    c.UpdatedAt,
                    c.CreatedBy
                }).ToListAsync();

            var userIds = rawItems.Select(x => x.CreatedBy).Distinct().ToList();
            var userMap = await _db.Users.IgnoreQueryFilters()
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.FullName);

            var items = rawItems.Select(c => new AppConfigDto
            {
                ConfigId = c.ConfigId,
                ConfigKey = c.ConfigKey,
                ConfigValue = c.ConfigValue,
                Description = c.Description,
                Platform = c.Platform,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                CreatedByName = userMap.TryGetValue(c.CreatedBy, out var name) ? name : "System"
            }).ToList();

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
            var schoolId = _currentUserService.SchoolId;

            if (schoolId.HasValue)
            {
                // First try getting school-specific config
                var val = await _db.AppConfigurations
                    .Where(c => c.SchoolId == schoolId.Value && c.ConfigKey == configKey && c.IsActive)
                    .Select(c => c.ConfigValue)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrWhiteSpace(val)) return val;
            }

            // Fallback to SchoolId = 1 or null template config
            return await _db.AppConfigurations
                .IgnoreQueryFilters()
                .Where(c => (c.SchoolId == 1 || c.SchoolId == null) && c.ConfigKey == configKey && c.IsActive)
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
                .FirstOrDefaultAsync(x => x.ConfigId == configId);

            if (c is null)
                return ApiResponse<AppConfigDto>.Fail("Configuration not found.");

            var creator = await _db.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.UserId == c.CreatedBy);

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
                CreatedByName = creator?.FullName ?? "System"
            });
        }

        public async Task<ApiResponse<bool>> CreateAsync(CreateAppConfigDto dto, int createdBy)
        {
            var schoolId = _currentUserService.SchoolId;

            // Ensure unique key per platform for THIS school
            var exists = await _db.AppConfigurations
                .AnyAsync(c => c.ConfigKey == dto.ConfigKey && c.Platform == dto.PlatformEnum && c.SchoolId == schoolId);
            if (exists)
                return ApiResponse<bool>.Fail($"Key '{dto.ConfigKey}' already exists for platform '{dto.Platform}'.");

            _db.AppConfigurations.Add(new AppConfiguration
            {
                SchoolId = schoolId,
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

            // Check uniqueness only if key or platform changed for THIS school
            if (c.ConfigKey != dto.ConfigKey || c.Platform != dto.PlatformEnum)
            {
                var exists = await _db.AppConfigurations
                    .AnyAsync(x => x.ConfigKey == dto.ConfigKey
                                && x.Platform == dto.PlatformEnum
                                && x.SchoolId == c.SchoolId
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