using CoreLocation;

namespace BusTracking.Mobile.Platforms.iOS
{
    /// <summary>
    /// iOS implementation of IBackgroundLocationService.
    ///
    /// Uses CLLocationManager with AllowsBackgroundLocationUpdates = true
    /// which is the same mechanism used by Uber, Ola, and Google Maps.
    ///
    /// Requires Info.plist entries:
    ///   NSLocationAlwaysAndWhenInUseUsageDescription
    ///   NSLocationAlwaysUsageDescription
    ///   UIBackgroundModes → location
    /// </summary>
    public class BackgroundLocationService : IBackgroundLocationService, IDisposable
    {
        private CLLocationManager? _locationManager;
        private Action<double, double, double?, double?>? _onLocation;
        public bool IsRunning { get; private set; }

        public async Task StartAsync(int tripId,
            Action<double, double, double?, double?> onLocation)
        {
            if (IsRunning) return;
            _onLocation = onLocation;

            // Request Always permission (required for background updates)
            var status = await Permissions.RequestAsync<Permissions.LocationAlways>();
            if (status != PermissionStatus.Granted)
                throw new Exception(
                    "Background location permission denied. " +
                    "Please allow location access 'Always' in Settings.");

            _locationManager = new CLLocationManager
            {
                DesiredAccuracy = CLLocation.AccuracyBest,
                DistanceFilter = 10,            // update every 10 meters
                AllowsBackgroundLocationUpdates = true,       // KEY: keeps GPS alive
                PausesLocationUpdatesAutomatically = false,   // don't pause when still
                ShowsBackgroundLocationIndicator = true       // blue bar shown to user
            };

            _locationManager.LocationsUpdated += OnLocationsUpdated;
            _locationManager.StartUpdatingLocation();

            IsRunning = true;
        }

        public Task StopAsync()
        {
            if (!IsRunning) return Task.CompletedTask;
            _locationManager?.StopUpdatingLocation();
            if (_locationManager is not null)
                _locationManager.LocationsUpdated -= OnLocationsUpdated;
            IsRunning = false;
            return Task.CompletedTask;
        }

        private void OnLocationsUpdated(object? sender, CLLocationsUpdatedEventArgs e)
        {
            var loc = e.Locations.LastOrDefault();
            if (loc is null) return;

            _onLocation?.Invoke(
                loc.Coordinate.Latitude,
                loc.Coordinate.Longitude,
                loc.Speed < 0 ? null : loc.Speed * 3.6,      // m/s → km/h
                loc.Course < 0 ? null : (double?)loc.Course);
        }

        public void Dispose()
        {
            _locationManager?.Dispose();
            _locationManager = null;
        }
    }
}
