using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Plugin.Firebase.CloudMessaging;

namespace BusTracking.Mobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            try
            {
                if (Intent != null)
                {
                    FirebaseCloudMessagingImplementation.OnNewIntent(Intent);
                }
                CreateNotificationChannel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainActivity] OnCreate Exception: {ex.Message}");
            }
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            try
            {
                if (intent != null)
                {
                    FirebaseCloudMessagingImplementation.OnNewIntent(intent);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainActivity] OnNewIntent Exception: {ex.Message}");
            }
        }

        private void CreateNotificationChannel()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var channelId = $"{PackageName}.general";
                    var notificationManager = (NotificationManager?)GetSystemService(NotificationService);
                    var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
                    notificationManager?.CreateNotificationChannel(channel);
                    FirebaseCloudMessagingImplementation.ChannelId = channelId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainActivity] CreateNotificationChannel Exception: {ex.Message}");
            }
        }
    }
}
