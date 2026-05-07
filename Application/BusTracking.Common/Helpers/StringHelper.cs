namespace BusTracking.Common.Helpers
{
    public static class StringHelper
    {
        public static string Truncate(this string? value, int maxLength, string suffix = "…")
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value[..maxLength] + suffix;
        }

        public static string ToInitials(this string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 1
                ? parts[0][..Math.Min(2, parts[0].Length)].ToUpper()
                : $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        }

        public static string MaskEmail(this string email)
        {
            if (string.IsNullOrEmpty(email)) return email;
            var at = email.IndexOf('@');
            if (at <= 1) return email;
            return $"{email[0]}***{email[(at - 1)..]}";
        }

        public static string ToSlug(this string text)
            => text.Trim().ToLower().Replace(" ", "-");

        public static bool IsNullOrEmpty(this string? value)
            => string.IsNullOrWhiteSpace(value);
    }
}
