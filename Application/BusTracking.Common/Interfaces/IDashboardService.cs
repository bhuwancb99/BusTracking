namespace BusTracking.Common.Interfaces
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync();
    }
}
