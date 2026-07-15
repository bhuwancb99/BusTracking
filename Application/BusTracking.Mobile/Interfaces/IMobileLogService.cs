namespace BusTracking.Mobile.Interfaces
{
    public interface IMobileLogService
    {
        Task LogExceptionAsync(Exception ex, string? moduleName = null, string? actionName = null, string? additionalDetails = null);
        Task LogEventAsync(string message, string? moduleName = null, string? actionName = null, string? additionalDetails = null);
    }
}
