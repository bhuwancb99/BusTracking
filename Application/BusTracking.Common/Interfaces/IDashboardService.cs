using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Dashboard;

namespace BusTracking.Common.Interfaces
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync();
    }
}
