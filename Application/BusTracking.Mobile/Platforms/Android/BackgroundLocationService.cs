using Android.Content;
using Android.OS;

namespace BusTracking.Mobile.Platforms.Android
{
    public class BackgroundLocationService : IBackgroundLocationService
    {
        public bool IsRunning { get; private set; }

        public async Task StartAsync(int tripId,
            Action<double, double, double?, double?> onLocation)
        {
            if (IsRunning) return;

            // Request foreground location first
            var fine = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (fine != PermissionStatus.Granted)
                throw new Exception("Location permission denied.");

            // Android 10 (API 29)+ requires explicit background location permission
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                var always = await Permissions.RequestAsync<Permissions.LocationAlways>();
                if (always != PermissionStatus.Granted)
                    throw new Exception(
                        "Background location denied. " +
                        "Go to Settings → App → Permissions → Location → Allow all the time.");
            }

            LocationForegroundService.OnLocationReceived = onLocation;

            var context = global::Android.App.Application.Context;
            var intent = new Intent(context, typeof(LocationForegroundService));
            intent.SetAction(LocationForegroundService.ActionStart);
            intent.PutExtra(LocationForegroundService.ExtraTripId, tripId);

            // StartForegroundService requires API 26+ — use version guard
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
#pragma warning disable CA1416 // Validate platform compatibility
                context.StartForegroundService(intent);
#pragma warning restore CA1416 // Validate platform compatibility
            else
                context.StartService(intent);

            IsRunning = true;
        }

        public Task StopAsync()
        {
            if (!IsRunning) return Task.CompletedTask;

            LocationForegroundService.OnLocationReceived = null;

            var context = global::Android.App.Application.Context;
            var intent = new Intent(context, typeof(LocationForegroundService));
            intent.SetAction(LocationForegroundService.ActionStop);
            context.StartService(intent);

            IsRunning = false;
            return Task.CompletedTask;
        }
    }
}
