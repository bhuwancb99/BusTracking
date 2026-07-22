namespace BusTracking.Common.Helpers
{
    public static class AppConstants
    {
        // Roles
        public const string RoleSuperAdmin = "SuperAdmin";
        public const string RoleBusCoordinator = "BusCoordinator";
        public const string RoleDriver = "Driver";
        public const string RoleParent = "Parent";
        public const string RoleStudent = "Student";

        // Admin roles (can manage entities)
        public const string AdminRoles = "SuperAdmin,BusCoordinator";

        // Password
        public const int MinPasswordLength = 8;
        public const int RandomPasswordLength = 12;
        public const int PasswordResetTokenHours = 2;

        // Paging defaults
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;

        public const string AppConfigPageSizeKey = "AppConfigPageSize";
        public const string AppConfigTrackingHubUrlKey = "TrackingHubUrl";

        /// <summary>
        /// Formats base API URL from AppConfig (e.g. "https://xyz.xyz.com")
        /// by appending "/hubs/tracking". Checks if URL ends with "/" to append "hubs/tracking" or "/hubs/tracking".
        /// </summary>
        public static string FormatTrackingHubUrl(string? rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl)) return "";
            var trimmed = rawUrl.Trim();
            if (trimmed.EndsWith("/hubs/tracking", System.StringComparison.OrdinalIgnoreCase))
                return trimmed;
            return trimmed.EndsWith("/") ? $"{trimmed}hubs/tracking" : $"{trimmed}/hubs/tracking";
        }

        // GPS ping interval (seconds) — used in MAUI
        public const int GpsPingIntervalSeconds = 10;

        // Notification titles
        public const string NotifTitleBusApproaching = "Bus Approaching!";
        public const string NotifTitleStudentPickedUp = "Student Picked Up";
        public const string NotifTitleNoShow = "Student No Show";
        public const string NotifTitleBusAssigned = "Bus Assignment Updated";
        public const string NotifTitleRouteChanged = "Route Changed";
        public const string NotifTitleBroadcast = "Announcement";

        // Email subjects
        public const string EmailSubjectWelcome = "Welcome to Bus Tracking System";
        public const string EmailSubjectPasswordReset = "Reset Your Password";
        public const string EmailSubjectBusAssigned = "Your Bus Has Been Assigned";

        // Date/time formats
        public const string DateFormat = "dd MMM yyyy";
        public const string TimeFormat = "hh:mm tt";
        public const string DateTimeFormat = "dd MMM yyyy hh:mm tt";
    }
}
