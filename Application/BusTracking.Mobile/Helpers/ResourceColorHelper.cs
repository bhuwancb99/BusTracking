namespace BusTracking.Mobile.Helpers
{
    /// <summary>
    /// ResourceColorHelper
    /// </summary>
    public static class ResourceColorHelper
    {
        /// <summary>
        /// GetColor
        /// </summary>
        /// <param name="lightKey"></param>
        /// <param name="darkKey"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public static string GetColor(string lightKey, string darkKey, string? fallback = null)
        {
            try
            {
                var app = Application.Current;

                if (app?.Resources == null)
                    return fallback ?? "#ffffff";

                // Detect current theme
                var isDark = app.RequestedTheme == AppTheme.Dark;

                var keyToUse = isDark ? darkKey : lightKey;

                if (app.Resources.TryGetValue(keyToUse, out var value) && value is Color color)
                    return color.ToHex();
            }
            catch
            {
                // optional logging
            }

            return fallback ?? "#ffffff";
        }
    }
}
