#if ANDROID
using Android.Content;
using Android.Media;
#elif IOS
using AVFoundation;
using Foundation;
#endif

namespace BusTracking.Mobile.Services
{
    public class RingtoneService : IRingtoneService
    {
#if ANDROID
        private Ringtone? _ringtone;
        private MediaPlayer? _mediaPlayer;
#elif IOS
        private AVAudioPlayer? _audioPlayer;
#endif

        public void PlayRingtone()
        {
            try
            {
#if ANDROID
                var context = Android.App.Application.Context;
                var alertUri = RingtoneManager.GetDefaultUri(RingtoneType.Ringtone)
                            ?? RingtoneManager.GetDefaultUri(RingtoneType.Notification);

                if (alertUri != null)
                {
                    StopRingtone();
                    _mediaPlayer = MediaPlayer.Create(context, alertUri);
                    if (_mediaPlayer != null)
                    {
                        _mediaPlayer.Looping = true;
                        _mediaPlayer.Start();
                    }
                }
#elif IOS
                var soundUri = NSUrl.FromFilename("/System/Library/Audio/UISounds/alarm.caf") 
                            ?? NSUrl.FromFilename("/System/Library/Audio/UISounds/ringtone.caf");
                if (soundUri != null)
                {
                    StopRingtone();
                    _audioPlayer = AVAudioPlayer.FromUrl(soundUri);
                    if (_audioPlayer != null)
                    {
                        _audioPlayer.NumberOfLoops = -1; // Infinite loop until stopped
                        _audioPlayer.Play();
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RingtoneService] PlayRingtone Exception: {ex.Message}");
            }
        }

        public void StopRingtone()
        {
            try
            {
#if ANDROID
                if (_mediaPlayer != null)
                {
                    if (_mediaPlayer.IsPlaying)
                    {
                        _mediaPlayer.Stop();
                    }
                    _mediaPlayer.Release();
                    _mediaPlayer.Dispose();
                    _mediaPlayer = null;
                }
#elif IOS
                if (_audioPlayer != null)
                {
                    if (_audioPlayer.Playing)
                    {
                        _audioPlayer.Stop();
                    }
                    _audioPlayer.Dispose();
                    _audioPlayer = null;
                }
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RingtoneService] StopRingtone Exception: {ex.Message}");
            }
        }
    }
}
