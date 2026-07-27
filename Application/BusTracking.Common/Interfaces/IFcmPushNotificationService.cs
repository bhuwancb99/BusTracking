namespace BusTracking.Common.Interfaces
{
    public interface IFcmPushNotificationService
    {
        Task SendTripStartedPushAsync(int tripId, int driverUserId);
        Task SendStudentPickedUpPushAsync(int tripId, int studentId, int stopId);
        Task SendBroadcastPushAsync(List<int> recipientUserIds, string title, string body, string notificationType);
    }
}
