namespace BusTracking.Common.Interfaces
{
    public interface INotificationService
    {
        Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(int userId);
        Task<ApiResponse<PagedResult<NotificationDto>>> GetUserNotificationsPagedAsync(int userId, int page = 1, DateTime? fromDate = null, DateTime? toDate = null);
        Task<ApiResponse<bool>> MarkAsReadAsync(int notificationId, int userId);
        Task<ApiResponse<bool>> MarkAllAsReadAsync(int userId);
        Task SendAsync(int recipientUserId, string title, string body, string type, int? referenceId = null);
    }
}
