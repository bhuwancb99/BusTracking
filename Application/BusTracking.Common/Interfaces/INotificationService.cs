namespace BusTracking.Common.Interfaces
{
    public interface INotificationService
    {
        Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(int userId);
        Task<ApiResponse<bool>> MarkAsReadAsync(int notificationId, int userId);
        Task<ApiResponse<bool>> MarkAllAsReadAsync(int userId);
        Task SendAsync(int recipientUserId, string title, string body, string type, int? referenceId = null);
    }
}
