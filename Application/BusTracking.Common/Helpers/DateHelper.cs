namespace BusTracking.Common.Helpers
{
    public static class DateHelper
    {
        public static string ToDisplay(this DateTime dt)
            => dt.ToString(AppConstants.DateTimeFormat);

        public static string ToDateDisplay(this DateTime dt)
            => dt.ToString(AppConstants.DateFormat);

        public static string ToTimeDisplay(this DateTime dt)
            => dt.ToString(AppConstants.TimeFormat);

        public static string ToDisplay(this DateOnly d)
            => d.ToString(AppConstants.DateFormat);

        public static string ToDisplay(this TimeOnly t)
            => t.ToString(AppConstants.TimeFormat);

        public static string? ToDisplay(this TimeOnly? t)
            => t?.ToString(AppConstants.TimeFormat);

        public static string TimeAgo(this DateTime dt)
        {
            var diff = DateTime.UtcNow - dt;
            return diff.TotalSeconds switch
            {
                < 60 => "just now",
                < 3600 => $"{(int)diff.TotalMinutes}m ago",
                < 86400 => $"{(int)diff.TotalHours}h ago",
                _ => $"{(int)diff.TotalDays}d ago"
            };
        }

        public static bool IsToday(this DateOnly d)
            => d == DateOnly.FromDateTime(DateTime.Today);

        public static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
    }
}
