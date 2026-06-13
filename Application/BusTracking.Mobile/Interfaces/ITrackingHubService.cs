namespace BusTracking.Mobile.Interfaces
{
    /// <summary>
    /// Wraps the SignalR HubConnection for real-time bus tracking.
    /// Used by both the Driver (sends location) and Parent/Student (receives location).
    /// </summary>
    public interface ITrackingHubService
    {
        bool IsConnected { get; }

        // ── Connection lifecycle ────────────────────────────────────────────
        Task ConnectAsync(string jwtToken);
        Task DisconnectAsync();

        // ── Driver side ─────────────────────────────────────────────────────
        Task JoinAsDriverAsync(int tripId);
        Task SendLocationAsync(int tripId, int busId,
            decimal lat, decimal lng, decimal? speed, decimal? heading);
        Task NotifyTripEndedAsync(int tripId);

        // ── Watcher side (parent / student / coordinator) ───────────────────
        Task WatchTripAsync(int tripId);
        Task StopWatchingAsync(int tripId);

        // ── Events raised on the consumer (ViewModel) ───────────────────────
        event Action<decimal, decimal, decimal?, decimal?, string> OnLocationReceived;
        event Action<int> OnTripEnded;
        event Action<string?> OnConnectionStateChanged;
    }
}
