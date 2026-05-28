namespace BusTracking.Mobile.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiService _api;
        private readonly LocalDatabase _db;
        private readonly ICacheService _cache;
        private SessionUser? _currentUser;

        public string CurrentRole => _currentUser?.Role ?? "";

        public AuthService(IApiService api, LocalDatabase db, ICacheService cache)
        {
            _api = api;
            _db = db;
            _cache = cache;
        }

        // ── Login ─────────────────────────────────────────────────────────────
        public async Task<ApiResponse<SessionUser>> LoginAsync(string email, string password)
        {
            var r = await _api.PostAsync<SessionUser>(Constants.Auth.Login,
                new LoginRequest
                {
                    Email = email,
                    Password = password
                });

            if (!r.Success || r.Data is null)
                return ApiResponse<SessionUser>.Fail(r.Message);

            var user = r.Data;

            // Clear any old/corrupt session before saving new one
            await _db.ClearSessionAsync();

            // Save encrypted to local DB
            await _db.SaveSessionAsync(user);

            // Set token on HttpClient
            _api.SetToken(user.Token);

            // Cache in memory
            _currentUser = user;

            return new ApiResponse<SessionUser>
            {
                Success = true,
                Data = user,
                Message = "Login successful."
            };
        }

        // ── Get current user (from memory → DB) ──────────────────────────────
        public async Task<SessionUser?> GetCurrentUserAsync()
        {
            // Return from memory if still valid
            if (_currentUser != null && DateTime.UtcNow < _currentUser.Expiry)
                return _currentUser;

            // Try restoring from DB (handles app close/reopen)
            try
            {
                var session = await _db.GetSessionAsync();
                if (session is null) 
                    return null;

                // Validate token is not empty/corrupt after decrypt
                if (string.IsNullOrWhiteSpace(session.Token))
                {
                    await _db.ClearSessionAsync();
                    return null;
                }

                _currentUser = session;
                _api.SetToken(session.Token);
                return session;
            }
            catch
            {
                // Corrupt DB session — wipe it so user gets a clean login
                await _db.ClearSessionAsync();
                return null;
            }
        }

        // ── Auth check ────────────────────────────────────────────────────────
        public async Task<bool> IsAuthenticatedAsync()
        {
            // Check memory first
            if (_currentUser != null && DateTime.UtcNow < _currentUser.Expiry)
                return true;

            // Try full restore — this also sets token on HttpClient
            var user = await GetCurrentUserAsync();
            return user is not null;
        }

        // ── Logout ────────────────────────────────────────────────────────────
        public async Task LogoutAsync()
        {
            _currentUser = null;
            _api.ClearToken();
            _cache.Clear();
            await _db.ClearSessionAsync();
        }

        // ── Permissions ───────────────────────────────────────────────────────
        public bool HasPermission(string permissionKey)
        {
            if (_currentUser is null)
                return false;
            if (_currentUser.Role == Constants.Roles.SuperAdmin)
                return true;
            if (string.IsNullOrEmpty(_currentUser.Permissions))
                return false;
            try
            {
                var perms = JsonSerializer.Deserialize<List<string>>(_currentUser.Permissions) ?? [];
                return perms.Contains(permissionKey, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        // ── Change password ───────────────────────────────────────────────────
        public async Task<ApiResponse<bool>> ChangePasswordAsync(string current, string newPwd)
        {
            return await _api.PostAsync<bool>(Constants.Auth.ChangePassword,
                new { CurrentPassword = current, NewPassword = newPwd, ConfirmPassword = newPwd });
        }

        // ── Forgot password ───────────────────────────────────────────────────
        public async Task<ApiResponse<bool>> ForgotPasswordAsync(string email)
        {
            return await _api.PostAsync<bool>(Constants.Auth.ForgotPassword, new { Email = email });
        }
    }
}