namespace BusTracking.Mobile;

/// <summary>
/// Single source of truth for all app-wide constants.
/// Change ApiBaseUrl to point to your deployed server.
/// </summary>
public static class Constants
{
    // ── API Base ──────────────────────────────────────────────────────────
#if DEBUG
    // Android emulator uses 10.0.2.2 to reach localhost
    public const string ApiBaseUrl = "https://10.0.2.2:7001";
#else
    public const string ApiBaseUrl = "https://api.bustracking.com";
#endif

    // ── Auth endpoints ────────────────────────────────────────────────────
    public static class Auth
    {
        public const string Login = "/api/auth/login";
        public const string Me = "/api/auth/me";
        public const string ChangePassword = "/api/auth/change-password";
        public const string ForgotPassword = "/api/auth/forgot-password";
        public const string ResetPassword = "/api/auth/reset-password";
    }

    // ── App Config (no auth required) ─────────────────────────────────────
    public static class AppConfig
    {
        public const string Mobile = "/api/app-config/mobile";
        public const string MobileKey = "/api/app-config/mobile/{0}";   // {0} = key
    }

    // ── SuperAdmin endpoints ──────────────────────────────────────────────
    public static class Admin
    {
        public const string Dashboard = "/api/admin/dashboard";

        public const string Buses = "/api/admin/buses";
        public const string BusById = "/api/admin/buses/{0}";
        public const string BusToggle = "/api/admin/buses/{0}/toggle";
        public const string BusAssignDriver = "/api/admin/buses/{0}/assign-driver";
        public const string BusDropdown = "/api/admin/buses/dropdown";

        public const string Routes = "/api/admin/routes";
        public const string RouteById = "/api/admin/routes/{0}";
        public const string RouteStops = "/api/admin/routes/{0}/stops";
        public const string RouteStopDelete = "/api/admin/routes/stops/{0}";

        public const string Drivers = "/api/admin/drivers";
        public const string DriverById = "/api/admin/drivers/{0}";
        public const string DriverToggle = "/api/admin/drivers/{0}/toggle";
        public const string DriverReset = "/api/admin/drivers/{0}/reset-password";
        public const string DriverDropdown = "/api/admin/drivers/dropdown";

        public const string Parents = "/api/admin/parents";
        public const string ParentById = "/api/admin/parents/{0}";
        public const string ParentToggle = "/api/admin/parents/{0}/toggle";
        public const string ParentReset = "/api/admin/parents/{0}/reset-password";

        public const string Students = "/api/admin/students";
        public const string StudentById = "/api/admin/students/{0}";
        public const string StudentToggle = "/api/admin/students/{0}/toggle";
        public const string StudentReset = "/api/admin/students/{0}/reset-password";
        public const string StudentSearch = "/api/admin/students/search";
        public const string StudentAvail = "/api/admin/students/{0}/availability";

        public const string Coordinators = "/api/admin/coordinators";
        public const string CoordinatorById = "/api/admin/coordinators/{0}";
        public const string CoordinatorToggle = "/api/admin/coordinators/{0}/toggle";
        public const string CoordinatorReset = "/api/admin/coordinators/{0}/reset-password";
        public const string CoordinatorPerms = "/api/admin/coordinators/{0}/permissions";
        public const string AllPermissions = "/api/admin/permissions";

        public const string Trips = "/api/admin/trips";
        public const string TripById = "/api/admin/trips/{0}";
        public const string TripStart = "/api/admin/trips/{0}/start";
        public const string TripEnd = "/api/admin/trips/{0}/end";
        public const string TripCancel = "/api/admin/trips/{0}/cancel";
        public const string TripStudents = "/api/admin/trips/{0}/students";
        public const string TripStops = "/api/admin/trips/{0}/stops";
        public const string TripLocation = "/api/admin/trips/{0}/location";
        public const string TripLocationHist = "/api/admin/trips/{0}/location/history";

        public const string Config = "/api/admin/config";
        public const string ConfigById = "/api/admin/config/{0}";
        public const string ConfigToggle = "/api/admin/config/{0}/toggle";

        public const string Feedback = "/api/admin/feedback";
        public const string FeedbackStatus = "/api/admin/feedback/{0}/status";

        public const string Notifications = "/api/admin/notifications";
        public const string NotifRead = "/api/admin/notifications/{0}/read";
        public const string NotifReadAll = "/api/admin/notifications/read-all";
        public const string NotifSend = "/api/admin/notifications/send";
    }

    // ── Coordinator endpoints ─────────────────────────────────────────────
    public static class Coordinator
    {
        public const string Dashboard = "/api/coordinator/dashboard";
        public const string Trips = "/api/coordinator/trips";
        public const string TripById = "/api/coordinator/trips/{0}";
        public const string TripStart = "/api/coordinator/trips/{0}/start";
        public const string TripEnd = "/api/coordinator/trips/{0}/end";
        public const string TripCancel = "/api/coordinator/trips/{0}/cancel";
        public const string TripLocation = "/api/coordinator/trips/{0}/location";
        public const string TripLocHist = "/api/coordinator/trips/{0}/location/history";
        public const string Buses = "/api/coordinator/buses";
        public const string BusById = "/api/coordinator/buses/{0}";
        public const string Routes = "/api/coordinator/routes";
        public const string RouteStops = "/api/coordinator/routes/{0}/stops";
        public const string Drivers = "/api/coordinator/drivers";
        public const string Parents = "/api/coordinator/parents";
        public const string Students = "/api/coordinator/students";
    }

    // ── Parent endpoints ──────────────────────────────────────────────────
    public static class Parent
    {
        public const string Dashboard = "/api/parent/dashboard";
        public const string TrackBus = "/api/parent/children/{0}/track";
        public const string Availability = "/api/parent/children/{0}/availability";
        public const string TripHistory = "/api/parent/trips/history";
    }

    // ── Student endpoints ─────────────────────────────────────────────────
    public static class Student
    {
        public const string Dashboard = "/api/student/dashboard";
        public const string Track = "/api/student/track";
        public const string Availability = "/api/student/availability";
    }

    // ── Common endpoints ──────────────────────────────────────────────────
    public static class Common
    {
        public const string Notifications = "/api/notifications";
        public const string DeviceToken = "/api/notifications/device-token";
        public const string Profile = "/api/profile";
        public const string Feedback = "/api/feedback";
    }

    // ── Local DB ──────────────────────────────────────────────────────────
    public static class Database
    {
        public const string Filename = "bustrack.db3";
        public const string EncryptionKey = "BusTrack@2024!SecureKey";
    }

    // ── Cache keys ────────────────────────────────────────────────────────
    public static class Cache
    {
        public const string AppConfig = "app_config_mobile";
        public const string Dashboard = "dashboard_summary";
        public const int AppConfigTtlH = 12;   // hours
        public const int DashboardTtlM = 5;    // minutes
        public const int ListTtlM = 3;    // minutes for lists
    }

    // ── App ───────────────────────────────────────────────────────────────
    public static class App
    {
        public const string TokenExpiryKey = "token_expiry";
        public const string UserKey = "current_user";
        public const string PermissionsKey = "user_permissions";
    }

    // ── Roles ─────────────────────────────────────────────────────────────
    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string BusCoordinator = "BusCoordinator";
        public const string Parent = "Parent";
        public const string Student = "Student";
        public const string Driver = "Driver";
    }
}