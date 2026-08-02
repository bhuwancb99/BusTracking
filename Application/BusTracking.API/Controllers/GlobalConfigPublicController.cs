namespace BusTracking.API.Controllers
{
    /// <summary>
    /// Public API — no authentication required.
    /// Returns active GlobalConfiguration key-value pairs (system-wide, NOT school-scoped).
    /// Used by MAUI apps BEFORE login for maintenance mode, forced update, version checks.
    /// </summary>
    [Route("api/global-config")]
    public class GlobalConfigPublicController : ApiBaseController
    {
        private readonly AppDbContext _db;

        public GlobalConfigPublicController(AppDbContext db)
        {
            _db = db;
        }

        // ── GET api/global-config/mobile ─────────────────────────────
        /// <summary>
        /// Returns all active GlobalConfiguration keys for pre-login use.
        /// No JWT required — called on app launch before user authenticates.
        /// Response example:
        /// {
        ///   "data": [
        ///     { "key": "IsMaintencePage",    "value": "false" },
        ///     { "key": "MandatoryUpdateApp", "value": "false" },
        ///     { "key": "AndroidVersion",     "value": "1.0.0" },
        ///     { "key": "iOSVersion",         "value": "1.0.0" },
        ///     { "key": "Android_Update_Url", "value": "https://play.google.com/…" },
        ///     { "key": "iOS_Update_Url",     "value": "https://apps.apple.com/…" }
        ///   ]
        /// }
        /// </summary>
        [HttpGet("mobile")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMobileGlobalConfig()
        {
            var globalConfigs = await _db.GlobalConfigurations.AsNoTracking()
                .Where(c => c.IsActive)
                .Select(c => new AppConfigValueDto { Key = c.GlobalConfigKey, Value = c.GlobalConfigValue })
                .ToListAsync();

            return Ok(ApiResponse<List<AppConfigValueDto>>.Ok(globalConfigs));
        }

        // ── GET api/global-config/mobile/{key} ──────────────────────
        /// <summary>
        /// Get a single GlobalConfiguration value by key.
        /// Returns 404 if key doesn't exist or is inactive.
        /// </summary>
        [HttpGet("mobile/{key}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMobileGlobalConfigKey(string key)
        {
            var item = await _db.GlobalConfigurations.AsNoTracking()
                .FirstOrDefaultAsync(c => c.GlobalConfigKey.ToLower() == key.ToLower() && c.IsActive);

            if (item is null)
                return NotFound(ApiResponse<object>.Fail($"Config key '{key}' not found or inactive."));

            return Ok(ApiResponse<object>.Ok(new { Key = item.GlobalConfigKey, Value = item.GlobalConfigValue }));
        }
    }
}
