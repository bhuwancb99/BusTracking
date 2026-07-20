using System.Collections.Concurrent;

namespace BusTracking.Common.Helpers
{
    public static class TimeZoneHelper
    {
        private static readonly ConcurrentDictionary<string, TimeZoneInfo> _tzCache = new(StringComparer.OrdinalIgnoreCase);
        private const string DefaultTimeZoneId = "India Standard Time";

        public static TimeZoneInfo GetTimeZoneInfo(string? timeZoneId)
        {
            var key = string.IsNullOrWhiteSpace(timeZoneId) ? DefaultTimeZoneId : timeZoneId.Trim();

            return _tzCache.GetOrAdd(key, tzKey =>
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(tzKey);
                }
                catch
                {
                    try
                    {
                        return TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZoneId);
                    }
                    catch
                    {
                        return TimeZoneInfo.Utc;
                    }
                }
            });
        }

        public static DateTime GetNow(string? timeZoneInfoId)
        {
            var tz = GetTimeZoneInfo(timeZoneInfoId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        public static DateOnly GetToday(string? timeZoneInfoId)
        {
            var now = GetNow(timeZoneInfoId);
            return DateOnly.FromDateTime(now);
        }

        public static DateOnly GetSchoolTodayDate(School? school)
        {
            var tz = GetTimeZoneInfo(school?.TimeZoneInfoId ?? school?.TimeZone?.WindowsTimeZoneId ?? school?.TimeZone?.IanaTimeZoneId);
            var schoolNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            return DateOnly.FromDateTime(schoolNow);
        }

        public static DateTime GetSchoolLocalDateTime(School? school, DateTime utcDateTime)
        {
            var tz = GetTimeZoneInfo(school?.TimeZoneInfoId ?? school?.TimeZone?.WindowsTimeZoneId ?? school?.TimeZone?.IanaTimeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.Kind == DateTimeKind.Utc ? utcDateTime : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), tz);
        }
    }
}
