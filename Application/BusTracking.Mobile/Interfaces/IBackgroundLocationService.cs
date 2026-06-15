namespace BusTracking.Mobile.Interfaces
{
    /// <summary>
    /// Keeps GPS pinging even when the screen is locked.
    /// Android: runs as a Foreground Service with a persistent notification.
    /// iOS:     runs using the "location" UIBackgroundMode.
    ///
    /// The driver app calls Start() when trip begins and Stop() when trip ends.
    /// Each GPS ping calls onLocation callback → ViewModel sends it to SignalR.
    /// </summary>
    public interface IBackgroundLocationService
    {
        bool IsRunning { get; }

        /// <summary>
        /// Start background GPS. onLocation is called every ~5 seconds
        /// with (latitude, longitude, speed, heading).
        /// </summary>
        Task StartAsync(int tripId, Action<double, double, double?, double?> onLocation);

        /// <summary>Stop GPS and remove the foreground notification.</summary>
        Task StopAsync();
    }
}
