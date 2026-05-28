namespace BusTracking.Mobile.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _http;
        private readonly LocalDatabase _db;
        private readonly ICacheService _cache;
        private string? _token;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ApiService(LocalDatabase db, ICacheService cache)
        {
            _db = db;
            _cache = cache;

#if DEBUG
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            _http = new HttpClient(handler)
            {
                BaseAddress = new Uri(Constants.ApiBaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
#else
    _http = new HttpClient()
    {
        BaseAddress = new Uri(Constants.ApiBaseUrl),
        Timeout = TimeSpan.FromSeconds(30)
    };
#endif
        }

        public void SetToken(string token)
        {
            _token = token;
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
                await EnsureTokenAsync();
                var res = await _http.GetAsync(endpoint);
                return await ParseAsync<T>(res);
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
                await EnsureTokenAsync();
                var content = body is null
                    ? new StringContent("{}", Encoding.UTF8, "application/json")
                    : new StringContent(JsonSerializer.Serialize(body, _json), Encoding.UTF8, "application/json");
                var res = await _http.PostAsync(endpoint, content);
                return await ParseAsync<T>(res);
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
                await EnsureTokenAsync();
                var content = body is null
                    ? new StringContent("{}", Encoding.UTF8, "application/json")
                    : new StringContent(JsonSerializer.Serialize(body, _json), Encoding.UTF8, "application/json");
                var res = await _http.PutAsync(endpoint, content);
                return await ParseAsync<T>(res);
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
                await EnsureTokenAsync();
                var res = await _http.DeleteAsync(endpoint);
                return await ParseAsync<T>(res);
            }
            catch (Exception ex)
            {
                return Fail<T>(ex.Message);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Load token from DB if not yet set in memory (app restart scenario)</summary>
        private async Task EnsureTokenAsync()
        {
            if (!string.IsNullOrEmpty(_token))
                return;
            var session = await _db.GetSessionAsync();
            if (session != null)
                SetToken(session.Token);
        }

        private async Task<ApiResponse<T>> ParseAsync<T>(HttpResponseMessage res)
        {
            var json = await res.Content.ReadAsStringAsync();

            if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Token expired — clear local session so app redirects to login
                await _db.ClearSessionAsync();
                ClearToken();
                return Fail<T>("Session expired. Please login again.");
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
