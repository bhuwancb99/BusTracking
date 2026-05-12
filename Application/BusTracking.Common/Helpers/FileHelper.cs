namespace BusTracking.Common.Helpers
{
    public static class FileHelper
    {
        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public static bool IsValidImage(string fileName, long fileSize)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return AllowedImageExtensions.Contains(ext) && fileSize <= MaxFileSizeBytes;
        }

        public static string GetProfileImagePath(string? profileImageUrl, string defaultImage = "/Images/avatar.svg")
            => string.IsNullOrEmpty(profileImageUrl) ? defaultImage : profileImageUrl;
    }
}
