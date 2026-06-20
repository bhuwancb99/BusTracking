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

        // AppConfiguration key that drives the App Configuration list's page size
        // (Web SuperAdmin/BusCoordinator + Mobile Admin/Coordinator). Seeded with
        // value "10" — see BusTrackingDB.sql. Falls back to DefaultPageSize if
        // the key is missing, inactive, or not a valid positive integer.
        public const string AppConfigPageSizeKey = "AppConfigPageSize";

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
