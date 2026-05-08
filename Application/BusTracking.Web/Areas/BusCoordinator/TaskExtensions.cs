namespace BusTracking.Web.Areas.BusCoordinator
{
    internal static class TaskExtensions
    {
        internal static async Task<T> Then<T>(this Task<BusTracking.Common.DTOs.Common.ApiResponse<T>> task)
            => (await task).Data!;
    }
}
