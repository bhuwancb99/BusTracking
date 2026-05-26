namespace BusTracking.Mobile.Models.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }

        public static ApiResponse<T> Fail(string msg) => new() { Success = false, Message = msg };
    }
}
