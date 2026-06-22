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
        public async Task<ApiResponse<SessionUser>> LoginAsync(string userName, string password)
        {
            var r = await _api.PostAsync<SessionUser>(Constants.Auth.Login,
                new LoginRequest
                {
                    UserName = userName,
                    Password = password
                });

            if (!r.Success || r.Data is null)
                return ApiResponse<SessionUser>.Fail(r.Message);

            var user = r.Data;

            // Clear any old/corrupt session before saving new one
            await _db.ClearSessionAsync();

            // Fetch profile image URL right after login
            _api.SetToken(user.Token);
            try
            {
                var profileResp = await _api.GetAsync<UserProfileDto>(Constants.Common.Profile);
                if (profileResp.Success && profileResp.Data is not null)
                    user.ProfileImageUrl = profileResp.Data.ProfileImageUrl;
            }
            catch { /* non-fatal — proceed without image */ }

            await _db.SaveSessionAsync(user);
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
            // Return cached user — but if it's a BusCoordinator with no permissions yet
            // (logged in before the API started sending them), fall through to patch it.
            if (_currentUser != null && DateTime.UtcNow < _currentUser.Expiry)
            {
                bool needsPermPatch = _currentUser.Role == Constants.Roles.BusCoordinator
                                   && string.IsNullOrWhiteSpace(_currentUser.Permissions);
                if (!needsPermPatch)
                    return _currentUser;
            }

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

                _api.SetToken(session.Token);

                // If this is a BusCoordinator with no permissions stored (old session
                // saved before the API started returning Permissions at login), fetch
                // them now and patch the stored session so the dashboard shows correctly.
                if (session.Role == Constants.Roles.BusCoordinator
                    && string.IsNullOrWhiteSpace(session.Permissions))
                {
                    try
                    {
                        // GET /api/admin/coordinators/{id}/permissions returns:
                        // { assignedPermissionIds: [1,3], allPermissions: [{id,key,...}] }
                        // Cross-reference to get the permission key strings.
                        var url = string.Format(Constants.Admin.CoordinatorPerms, session.UserId);
                        var permR = await _api.GetAsync<PermissionsResponse>(url);
                        if (permR.Success && permR.Data is not null)
                        {
                            var assignedKeys = permR.Data.AllPermissions
                                .Where(p => permR.Data.AssignedPermissionIds.Contains(p.Id))
                                .Select(p => p.Key)
                                .ToList();
                            session.Permissions = JsonSerializer.Serialize(assignedKeys);
                            await _db.SaveSessionAsync(session);
                        }
                    }
                    catch { }
                }

                _currentUser = session;
                return session;
            }
            catch
            {
                await _db.ClearSessionAsync();
                return null;
            }
        }

        // ── Update profile image URL in cached session ────────────────────────
        /// <summary>
        /// Called by ProfilePage after a successful photo upload or removal
        /// so the flyout avatar stays in sync without requiring a re-login.
        /// </summary>
        public async Task RefreshProfileImageAsync(string? newUrl)
        {
            if (_currentUser is not null)
            {
                _currentUser.ProfileImageUrl = newUrl;
                await _db.SaveSessionAsync(_currentUser);
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