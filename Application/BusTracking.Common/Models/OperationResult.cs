namespace BusTracking.Common.Models
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public List<string> Errors { get; set; } = [];

        public static OperationResult Ok(string message = "Operation successful.")
            => new() { Success = true, Message = message };

        public static OperationResult Fail(string message, params string[] errors)
            => new() { Success = false, Message = message, Errors = [.. errors] };
    }

    public class OperationResult<T> : OperationResult
    {
        public T? Data { get; set; }

        public static OperationResult<T> Ok(T data, string message = "Operation successful.")
            => new() { Success = true, Data = data, Message = message };

        public new static OperationResult<T> Fail(string message, params string[] errors)
            => new() { Success = false, Message = message, Errors = [.. errors] };
    }
}
