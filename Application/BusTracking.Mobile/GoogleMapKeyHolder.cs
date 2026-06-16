namespace BusTracking.Mobile
{
    /// <summary>
    /// Holds the Google Maps API key loaded at runtime from AppConfig.
    /// Avoids hardcoding the key in AndroidManifest.xml or Info.plist.
    ///
    /// SuperAdmin sets "GoogleMapApiKey" in the App Configuration screen.
    /// At app startup the key is fetched and stored here.
    /// The LiveTracking WebView reads it via GetMapHtml() before loading.
    /// </summary>
    public static class GoogleMapKeyHolder
    {
        /// <summary>API key fetched from AppConfig at startup.</summary>
        public static string ApiKey { get; set; } = "";

        /// <summary>
        /// Returns the tracking_map.html content with the real API key injected.
        /// Called by LiveTrackingPage before loading the WebView.
        /// </summary>
        public static async Task<string> GetMapHtmlAsync()
        {
            // Read the base HTML from Resources/Raw
            using var stream = await FileSystem.OpenAppPackageFileAsync("tracking_map.html");
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();

            // Replace the placeholder with the runtime key from AppConfig
            return html.Replace("YOUR_GOOGLE_MAPS_API_KEY", ApiKey);
        }
    }
}
