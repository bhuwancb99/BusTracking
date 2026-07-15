namespace BusTracking.Common.Interfaces
{
    public interface ILogService
    {
        Task LogAsync(
            string platform,
            string? exceptionMessage,
            string? stackTrace,
            string? requestUrl,
            int? userId,
            string? username,
            string? role,
            string? moduleName,
            string? actionName,
            string? additionalDetails);
    }
}
