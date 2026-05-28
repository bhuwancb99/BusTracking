namespace BusTracking.Mobile.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _http;
        private readonly LocalDatabase _db;
        private readonly ICacheService _cache;
        private string? _token;

        // Endpoints that must NOT have a token attached
        private static readonly string[] _noAuthEndpoints =
        [
            Constants.Auth.Login,
            Constants.Auth.ForgotPassword,
            Constants.Auth.ResetPassword,
            Constants.AppConfig.Mobile,
        ];

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ApiService(LocalDatabase db, ICacheService cache)
        {
            _db = db;
            _cache = cache;

            var handler = new HttpClientHandler
            {
#if DEBUG
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
#endif
            };

            _http = new HttpClient(handler)
            {
                BaseAddress = new Uri(Constants.ApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public void SetToken(string token)
        {
            _token = token;
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearToken()
        {
            _token = null;
            _http.DefaultRequestHeaders.Authorization = null;
        }

        // ── GET ───────────────────────────────────────────────────────────────
        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                await EnsureTokenAsync(endpoint);
                var res = await _http.GetAsync(endpoint);
                return await ParseAsync<T>(res, endpoint);
            }
            catch (Exception ex)
            {
                return Fail<T>(ex.Message);
            }
        }

        // ── POST ──────────────────────────────────────────────────────────────
        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? body = null)
        {
            try
            {
                await EnsureTokenAsync(endpoint);
                var content = body is null
                    ? new StringContent("{}", Encoding.UTF8, "application/json")
                    : new StringContent(JsonSerializer.Serialize(body, _json), Encoding.UTF8, "application/json");
                var res = await _http.PostAsync(endpoint, content);
                return await ParseAsync<T>(res, endpoint);
            }
            catch (Exception ex)
            {
                return Fail<T>(ex.Message);
            }
        }

        // ── PUT ───────────────────────────────────────────────────────────────
        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? body = null)
        {
            try
            {
                await EnsureTokenAsync(endpoint);
                var content = body is null
                    ? new StringContent("{}", Encoding.UTF8, "application/json")
                    : new StringContent(JsonSerializer.Serialize(body, _json), Encoding.UTF8, "application/json");
                var res = await _http.PutAsync(endpoint, content);
                return await ParseAsync<T>(res, endpoint);
            }
            catch (Exception ex)
            {
                return Fail<T>(ex.Message);
            }
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
        {
            try
            {
                await EnsureTokenAsync(endpoint);
                var res = await _http.DeleteAsync(endpoint);
                return await ParseAsync<T>(res, endpoint);
            }
            catch (Exception ex)
            {
                return Fail<T>(ex.Message);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Restore token from DB on app restart.
        /// Skipped entirely for login/public endpoints that need no token.
        /// </summary>
        private async Task EnsureTokenAsync(string endpoint)
        {
            // Skip for public endpoints — login must never send a stale token
            if (_noAuthEndpoints.Any(e =>
                    endpoint.StartsWith(e, StringComparison.OrdinalIgnoreCase)))
            {
                // Also clear any stale token header so it is not accidentally sent
                _http.DefaultRequestHeaders.Authorization = null;
                return;
            }

            // Token already set in memory — nothing to do
            if (!string.IsNullOrEmpty(_token))
                return;

            // App restarted — restore token from DB
            try
            {
                var session = await _db.GetSessionAsync();
                if (session != null && !string.IsNullOrWhiteSpace(session.Token))
                    SetToken(session.Token);
            }
            catch
            {
                // DB read failed — proceed without token, server will return 401
            }
        }

        private async Task<ApiResponse<T>> ParseAsync<T>(HttpResponseMessage res, string endpoint)
        {
            var json = await res.Content.ReadAsStringAsync();

            if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Do not clear session for public endpoints (login returning 401 = wrong credentials)
                if (!_noAuthEndpoints.Any(e =>
                        endpoint.StartsWith(e, StringComparison.OrdinalIgnoreCase)))
                {
                    await _db.ClearSessionAsync();
                    ClearToken();
                    return Fail<T>("Session expired. Please login again.");
                }

                // Login endpoint returned 401 = wrong credentials
                try
                {
                    var err = JsonSerializer.Deserialize<ApiResponse<T>>(json, _json);
                    return err ?? Fail<T>("Invalid email or password.");
                }
                catch { return Fail<T>("Invalid email or password."); }
            }

            if (!res.IsSuccessStatusCode)
            {
                try
                {
                    var err = JsonSerializer.Deserialize<ApiResponse<T>>(json, _json);
                    return err ?? Fail<T>($"HTTP {(int)res.StatusCode}");
                }
                catch { return Fail<T>($"HTTP {(int)res.StatusCode}"); }
            }

            try
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(json, _json);
                return result ?? Fail<T>("Empty response");
            }
            catch (Exception ex)
            {
                return Fail<T>($"Parse error: {ex.Message}");
            }
        }

        private static ApiResponse<T> Fail<T>(string msg) => new() { Success = false, Message = msg };
    }
}