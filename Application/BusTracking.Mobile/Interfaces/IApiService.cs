namespace BusTracking.Mobile.Interfaces
{
    /// <summary>
    /// Central API service. All HTTP calls go through here.
    /// Automatically attaches JWT token, handles 401 token expiry,
    /// and routes errors back as ApiResponse failures.
    /// </summary>
    public interface IApiService
    {
        Task<ApiResponse<T>> GetAsync<T>(string endpoint);
        Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? body = null);
        Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? body = null);
        Task<ApiResponse<T>> DeleteAsync<T>(string endpoint);
        Task<ApiResponse<T>> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content);
        void SetToken(string token);
        void ClearToken();
    }
}
