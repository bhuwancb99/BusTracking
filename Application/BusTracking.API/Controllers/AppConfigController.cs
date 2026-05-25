namespace BusTracking.API.Controllers
{
    /// <summary>
    /// Public API — no authentication required.
    /// Used by MAUI apps on startup to fetch configuration values.
    /// </summary>
    [Route("api/app-config")]
    public class AppConfigController : ApiBaseController
    {
        private readonly IAppConfigService _config;
        public AppConfigController(IAppConfigService config) => _config = config;

        // ── GET api/app-config/mobile ────────────────────────────────
        /// <summary>
        /// Returns all active config keys for the Mobile platform.
        /// Call this on app startup — no JWT required.
        /// Response example:
        /// {
        ///   "data": [
        ///     { "key": "IsMaintencePage",    "value": "0" },
        ///     { "key": "MandatoryUpdateApp", "value": "0" },
        ///     { "key": "MinAppVersion",      "value": "1.0.0" },
        ///     { "key": "Android_Update_Url", "value": "https://play.google.com/…" },
        ///     { "key": "iOS_Update_Url",     "value": "https://apps.apple.com/…" },
        ///     { "key": "GpsIntervalSeconds", "value": "10" }
        ///   ]
        /// }
        /// </summary>
        [HttpGet("mobile")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMobileConfig()
        {
            var r = await _config.GetConfigForPlatformAsync(ConfigPlatform.Mobile);
            return Ok(r);
        }

        // ── GET api/app-config/mobile/{key} ─────────────────────────
        /// <summary>
        /// Get a single config value by key for Mobile.
        /// Returns 404 if key doesn't exist or is inactive.
        /// </summary>
        [HttpGet("mobile/{key}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMobileConfigKey(string key)
        {
            var all = await _config.GetConfigForPlatformAsync(ConfigPlatform.Mobile);
            var item = all.Data?.FirstOrDefault(c =>
                string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase));

            if (item is null)
                return NotFound(ApiResponse<object>.Fail($"Config key '{key}' not found or inactive."));

            return Ok(ApiResponse<object>.Ok(new { item.Key, item.Value }));
        }

        // ── GET api/app-config/web ───────────────────────────────────
        /// <summary>Returns all active config keys for the Web platform.</summary>
        [HttpGet("web")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetWebConfig()
        {
            var r = await _config.GetConfigForPlatformAsync(ConfigPlatform.Web);
            return Ok(r);
        }
    }
}
