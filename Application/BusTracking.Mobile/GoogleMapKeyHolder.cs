namespace BusTracking.Mobile
{
    /// <summary>
    /// Holds map configurations loaded at runtime from AppConfig.
    ///
    /// IsUseGoogleMap:
    ///   "1" = Paid Google Maps (uses GoogleMapApiKey)
    ///   "0" = Free Leaflet Map (loads OpenStreetMap directly without Google SDK)
    /// </summary>
    public static class GoogleMapKeyHolder
    {
        /// <summary>API key fetched from AppConfig at startup.</summary>
        public static string ApiKey { get; set; } = "";

        /// <summary>IsUseGoogleMap fetched from AppConfig: "1" = Paid Google Map, "0" = Free Leaflet Map.</summary>
        public static string IsUseGoogleMap { get; set; } = "0";

        /// <summary>
        /// Returns the tracking_map.html content with the dynamic script URL injected.
        /// Called by LiveTrackingPage and DriverTrackingPage before loading the WebView.
        /// </summary>
        public static async Task<string> GetMapHtmlAsync()
        {
            // Read the base HTML from Resources/Raw
            using var stream = await FileSystem.OpenAppPackageFileAsync("tracking_map.html");
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();

            var key = ApiKey?.Trim() ?? "";
            var isPaidGoogle = IsUseGoogleMap == "1" && !string.IsNullOrWhiteSpace(key);

            // IsUseGoogleMap = 1 -> Paid Google Maps (load SDK with API Key)
            // IsUseGoogleMap = 0 -> Free Leaflet Map (do not load Google SDK script)
            var scriptUrl = isPaidGoogle
                ? $"https://maps.googleapis.com/maps/api/js?key={key}&callback=initMap&libraries=geometry"
                : "";

            html = html.Replace("GOOGLE_MAPS_SCRIPT_URL", scriptUrl);
            html = html.Replace("YOUR_GOOGLE_MAPS_API_KEY", key);
            html = html.Replace("YOUR_IS_USE_GOOGLE_MAP", isPaidGoogle ? "1" : "0");
            return html;
        }
    }
}
