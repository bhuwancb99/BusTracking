using BusTracking.Mobile.Models.Dashboard;

namespace BusTracking.Mobile.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummary?> GetAdminSummaryAsync(bool forceRefresh = false);
    }
}
