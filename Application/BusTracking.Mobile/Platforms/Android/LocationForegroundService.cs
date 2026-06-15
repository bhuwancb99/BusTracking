using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;

namespace BusTracking.Mobile.Platforms.Android
{
    [Service(ForegroundServiceType = ForegroundService.TypeLocation)]
    public class LocationForegroundService : Service
    {
        public const string ActionStart = "ACTION_START_LOCATION";
        public const string ActionStop = "ACTION_STOP_LOCATION";
        public const string ExtraTripId = "TRIP_ID";
        private const int NotifId = 1001;
        private const string ChannelId = "bus_tracking_channel";

        private CancellationTokenSource? _cts;

        public static Action<double, double, double?, double?>? OnLocationReceived;

        public override IBinder? OnBind(Intent? intent) => null;

        public override StartCommandResult OnStartCommand(
            Intent? intent, StartCommandFlags flags, int startId)
        {
            if (intent?.Action == ActionStop)
            {
                StopSelf();
                return StartCommandResult.NotSticky;
            }

            CreateNotificationChannel();

            var notification = BuildNotification();

            // ForegroundService.TypeLocation requires API 29+
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
#pragma warning disable CA1416
                StartForeground(NotifId, notification, ForegroundService.TypeLocation);
#pragma warning restore CA1416
            }
            else
            {
                StartForeground(NotifId, notification);
            }

            _cts = new CancellationTokenSource();
            _ = RunGpsLoopAsync(_cts.Token);

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            _cts?.Cancel();
            base.OnDestroy();
        }

        // ── GPS loop — runs every 5 seconds ──────────────────────────────
        private async Task RunGpsLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var loc = await Geolocation.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.Best,
                                               TimeSpan.FromSeconds(4)));

                    if (loc is not null)
                        OnLocationReceived?.Invoke(
                            loc.Latitude,
                            loc.Longitude,
                            loc.Speed,
                            loc.Course);
                }
                catch { /* GPS errors are non-fatal */ }

                try { await Task.Delay(5_000, token); }
                catch (TaskCanceledException) { break; }
            }
        }

        // ── Notification ──────────────────────────────────────────────────
        private Notification BuildNotification()
        {
            var mainIntent = new Intent(this, typeof(BusTracking.Mobile.MainActivity));

            // PendingIntentFlags.Immutable is only available on API 23+
            PendingIntentFlags pendingFlags;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
#pragma warning disable CA1416
                pendingFlags = PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable;
#pragma warning restore CA1416
            }
            else
            {
                pendingFlags = PendingIntentFlags.UpdateCurrent;
            }

            // PendingIntent.GetActivity can return null on older APIs
            // NotificationCompat.Builder.SetContentIntent safely accepts null
            var pendingIntent = PendingIntent.GetActivity(
                this, 0, mainIntent, pendingFlags);

#pragma warning disable CS8602,CS8603 // Dereference of a possibly null reference.//CS8603 Possible null reference return.
            return new NotificationCompat.Builder(this, ChannelId)
                .SetContentTitle("Bus Tracking Active")
                .SetContentText("GPS is running. Parents can see your location.")
                .SetSmallIcon(Resource.Mipmap.appicon)
                .SetOngoing(true)
                .SetPriority(NotificationCompat.PriorityLow)
                .SetContentIntent(pendingIntent)
                .Build();
#pragma warning restore CS8602,CS8603 // Dereference of a possibly null reference.//CS8603 Possible null reference return.
        }

        // ── Notification channel (API 26+ only) ───────────────────────────
        private void CreateNotificationChannel()
        {
            // NotificationChannel + NotificationImportance + Description
            // all require API 26+ — one guard covers all warnings
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;

#pragma warning disable CA1416
            var channel = new NotificationChannel(
                ChannelId,
                "Bus Tracking",
                NotificationImportance.Low);

            channel.Description = "Shows while the driver is on an active trip.";

            var manager = GetSystemService(NotificationService) as NotificationManager;
            manager?.CreateNotificationChannel(channel);
#pragma warning restore CA1416
        }
    }
}
