namespace BusTracking.Common.Interfaces
{
    public interface IFcmPushNotificationService
    {
        Task SendTripStartedPushAsync(int tripId, int driverUserId);
        Task SendStudentPickedUpPushAsync(int tripId, int studentId, int stopId);
        Task SendStudentBoardingStatusPushAsync(int tripId, int studentId, int stopId, BoardingStatus status);
        Task SendBroadcastPushAsync(List<int> recipientUserIds, string title, string body, string notificationType);
    }
}
