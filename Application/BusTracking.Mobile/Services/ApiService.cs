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

            // Set global API version header on all requests
            _http.DefaultRequestHeaders.Remove("x-api-version");
            _http.DefaultRequestHeaders.Add("x-api-version", Constants.ApiVersion);
        }

        public void SetToken(string token)
        {
            _token = token;
            if (!string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _http.DefaultRequestHeaders.Authorization = null;
            }
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

        // ── POST MULTIPART (file upload) ──────────────────────────────────────
        public async Task<ApiResponse<T>> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content)
        {
            try
            {
                await EnsureTokenAsync(endpoint);
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
        /// Restore token from DB on app restart and ensure Authorization header is set for authenticated endpoints.
        /// Public endpoints (e.g. login) skip sending token.
        /// </summary>
        private async Task EnsureTokenAsync(string endpoint)
        {
            bool isNoAuth = _noAuthEndpoints.Any(e =>
                endpoint.StartsWith(e, StringComparison.OrdinalIgnoreCase));

            if (isNoAuth)
            {
                _http.DefaultRequestHeaders.Authorization = null;
                return;
            }

            // Restore token from DB if memory token is missing (e.g. app restart)
            if (string.IsNullOrWhiteSpace(_token))
            {
                try
                {
                    var session = await _db.GetSessionAsync();
                    if (session != null && !string.IsNullOrWhiteSpace(session.Token))
                    {
                        _token = session.Token;
                    }
                }
                catch
                {
                    // DB read failed — proceed without token
                }
            }

            // ALWAYS ensure the Bearer token header is set on _http for authenticated endpoints
            if (!string.IsNullOrWhiteSpace(_token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            }
            else
            {
                _http.DefaultRequestHeaders.Authorization = null;
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

            if (res.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                // Parse the message from API if available, else show default
                try
                {
                    var err = JsonSerializer.Deserialize<ApiResponse<T>>(json, _json);
                    return Fail<T>(err?.Message ?? "You don't have permission to access this feature.");
                }
                catch { return Fail<T>("You don't have permission to access this feature."); }
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