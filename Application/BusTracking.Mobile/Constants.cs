namespace BusTracking.Mobile;

/// <summary>
/// Single source of truth for all app-wide constants.
/// Change ApiBaseUrl to point to your deployed server.
/// </summary>
public static class Constants
{
    // ── API Base ──────────────────────────────────────────────────────────

#if DEBUG
    // Switch comment depending on what you are testing on
    public const string ApiBaseUrl = "https://10.0.2.2:7001";       // Android emulator
    //public const string ApiBaseUrl = "https://192.168.29.242:7001";  // Physical device
#else
    public const string ApiBaseUrl = "https://api.bustracking.com";
#endif

    public const string DeviceToken = "/api/notifications/device-token";

    // ── Auth  →  AuthController  [Route("api/[controller]")] ─────────────
    public static class Auth
    {
        public const string Login = "/api/auth/login";
        public const string Me = "/api/auth/me";
        public const string ChangePassword = "/api/auth/change-password";
        public const string ForgotPassword = "/api/auth/forgot-password";
        public const string ResetPassword = "/api/auth/reset-password";
        public const string CheckUsername = "/api/auth/check-username";
    }

    // ── App Config  →  AppConfigController  [Route("api/app-config")] ────
    public static class AppConfig
    {
        public const string Mobile = "/api/app-config/mobile";
        public const string MobileKey = "/api/app-config/mobile/{0}";
    }

    public static class Lookups
    {
        public const string Standards = "/api/standards";
    }

    // ── SuperAdmin  →  SuperAdminController  [Route("api/admin")] ────────
    public static class Admin
    {
        public const string Dashboard = "/api/admin/dashboard";

        // Buses
        public const string Buses = "/api/admin/buses";
        public const string BusById = "/api/admin/buses/{0}";
        public const string BusToggle = "/api/admin/buses/{0}/toggle";
        public const string BusAssignDriver = "/api/admin/buses/{0}/assign-driver";
        public const string BusDropdown = "/api/admin/buses/dropdown";

        // Routes
        public const string Routes = "/api/admin/routes";
        public const string RouteById = "/api/admin/routes/{0}";
        public const string RouteStops = "/api/admin/routes/{0}/stops";
        public const string RouteStopDelete = "/api/admin/routes/stops/{0}";
        public const string RouteReorderStops = "/api/admin/routes/{0}/stops/reorder";

        // Drivers
        public const string Drivers = "/api/admin/drivers";
        public const string DriverById = "/api/admin/drivers/{0}";
        public const string DriverToggle = "/api/admin/drivers/{0}/toggle";
        public const string DriverReset = "/api/admin/drivers/{0}/reset-password";
        public const string DriverDropdown = "/api/admin/drivers/dropdown";

        // Parents
        public const string Parents = "/api/admin/parents";
        public const string ParentById = "/api/admin/parents/{0}";
        public const string ParentToggle = "/api/admin/parents/{0}/toggle";
        public const string ParentReset = "/api/admin/parents/{0}/reset-password";

        // Students
        public const string Students = "/api/admin/students";
        public const string StudentById = "/api/admin/students/{0}";
        public const string StudentToggle = "/api/admin/students/{0}/toggle";
        public const string StudentReset = "/api/admin/students/{0}/reset-password";
        public const string StudentSearch = "/api/admin/students/search";
        public const string StudentAvail = "/api/admin/students/{0}/availability";

        // Coordinators
        public const string Coordinators = "/api/admin/coordinators";
        public const string CoordinatorById = "/api/admin/coordinators/{0}";
        public const string CoordinatorToggle = "/api/admin/coordinators/{0}/toggle";
        public const string CoordinatorReset = "/api/admin/coordinators/{0}/reset-password";
        public const string CoordinatorPerms = "/api/admin/coordinators/{0}/permissions";
        public const string AllPermissions = "/api/admin/permissions";

        // Trips
        public const string Trips = "/api/admin/trips";
        public const string TripById = "/api/admin/trips/{0}";
        public const string TripStart = "/api/admin/trips/{0}/start";
        public const string TripEnd = "/api/admin/trips/{0}/end";
        public const string TripCancel = "/api/admin/trips/{0}/cancel";
        public const string TripStudents = "/api/admin/trips/{0}/students";
        public const string TripStops = "/api/admin/trips/{0}/stops";
        public const string TripLocation = "/api/admin/trips/{0}/location";
        public const string TripLocationHist = "/api/admin/trips/{0}/location/history";

        // App Config (admin CRUD)
        public const string Config = "/api/admin/config";
        public const string ConfigById = "/api/admin/config/{0}";
        public const string ConfigToggle = "/api/admin/config/{0}/toggle";

        // Standards (admin CRUD)
        public const string Standards = "/api/admin/standards";
        public const string StandardById = "/api/admin/standards/{0}";
        public const string StandardToggle = "/api/admin/standards/{0}/toggle";

        // Feedback & Notifications
        public const string Feedback = "/api/admin/feedback";
        public const string FeedbackStatus = "/api/admin/feedback/{0}/status";
        public const string Notifications = "/api/admin/notifications";
        public const string NotifRead = "/api/admin/notifications/{0}/read";
        public const string NotifReadAll = "/api/admin/notifications/read-all";
        public const string NotifSend = "/api/admin/notifications/send";
    }

    // ── Coordinator  →  CoordinatorController  [Route("api/coordinator")] ─
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
        public const string RouteById = "/api/coordinator/routes/{0}";
        public const string RouteStops = "/api/coordinator/routes/{0}/stops";
        public const string RouteStopDelete = "/api/coordinator/routes/stops/{0}";
        public const string RouteReorderStops = "/api/coordinator/routes/{0}/stops/reorder";

        public const string Drivers = "/api/coordinator/drivers";
        public const string Parents = "/api/coordinator/parents";
        public const string ParentById = "/api/coordinator/parents/{0}";
        public const string ParentToggle = "/api/coordinator/parents/{0}/toggle";

        public const string Students = "/api/coordinator/students";
        public const string StudentById = "/api/coordinator/students/{0}";
        public const string StudentSearch = "/api/coordinator/students/search";

        public const string SubAdmins = "/api/coordinator/subadmins";
        public const string SubAdminById = "/api/coordinator/subadmins/{0}";
        public const string SubAdminToggle = "/api/coordinator/subadmins/{0}/toggle";
        public const string SubAdminReset = "/api/coordinator/subadmins/{0}/reset-password";
        public const string SubAdminPerms = "/api/coordinator/subadmins/{0}/permissions";
        public const string CoordAllPermissions = "/api/coordinator/permissions";

        public const string Config = "/api/coordinator/config";
        public const string ConfigById = "/api/coordinator/config/{0}";
        public const string ConfigToggle = "/api/coordinator/config/{0}/toggle";

        // Standards (coordinator CRUD)
        public const string Standards = "/api/coordinator/standards";
        public const string StandardById = "/api/coordinator/standards/{0}";
        public const string StandardToggle = "/api/coordinator/standards/{0}/toggle";

        public const string Feedback = "/api/coordinator/feedback";
        public const string FeedbackById = "/api/coordinator/feedback/{0}";
        public const string FeedbackUpdateStatus = "/api/coordinator/feedback/{0}/status";

        public const string CoordNotifications = "/api/coordinator/notifications";
        public const string CoordNotifMarkRead = "/api/coordinator/notifications/{0}/read";
        public const string CoordNotifMarkAllRead = "/api/coordinator/notifications/read-all";
    }

    // ── Driver ────────────────────────────────────────────────────────────
    public static class Driver
    {
        public const string Dashboard = "/api/driver/dashboard";
        public const string Trips = "/api/trips/my-trip";
        public const string TripStart = "/api/trips/{0}/start";
        public const string TripEnd = "/api/trips/{0}/end";
        public const string TripStops = "/api/trips/{0}/stops";
        public const string TripStudents = "/api/trips/{0}/students";
        public const string StopReach = "/api/trips/{0}/stops/{1}/reach";
        public const string StopDepart = "/api/trips/{0}/stops/{1}/depart";
        public const string TripBoarding = "/api/trips/{0}/boarding";
        public const string LocationPing = "/api/location/ping";
        public const string LocationLatest = "/api/location/{0}/latest";
        public const string Profile = "/api/driver/profile";
        public const string Notifications = "/api/driver/notifications";
        public const string NotifMarkRead = "/api/driver/notifications/{0}/read";
        public const string NotifMarkAllRead = "/api/driver/notifications/read-all";
    }

    // ── Student ───────────────────────────────────────────────────────────
    public static class Student
    {
        public const string Dashboard = "/api/student/dashboard";
        public const string Track = "/api/student/track";
        public const string Tracking = "/api/student/track";
        public const string Availability = "/api/student/availability";
    }

    // ── Parent ────────────────────────────────────────────────────────────
    public static class Parent
    {
        public const string Dashboard = "/api/parent/dashboard";
        public const string TrackBus = "/api/parent/children/{0}/track";
        public const string Availability = "/api/parent/children/{0}/availability";
        public const string TripHistory = "/api/parent/trips/history";
        public const string TripRouteInfo = "/api/parent/trips/{0}/route-info";
    }

    // ── Common  →  NotificationsController / FeedbackController / ProfileController ─
    public static class Common
    {
        public const string Notifications = "/api/notifications";
        public const string DeviceToken = "/api/notifications/device-token";
        public const string Profile = "/api/profile";
        public const string ProfilePhoto = "/api/profile/photo";   // POST multipart | DELETE
        public const string Feedback = "/api/feedback";
    }

    // SignalR hub URL (used by TrackingHubService)
    public const string TrackingHubUrl = ApiBaseUrl + "/hubs/tracking";

    // ── BusType  →  BusTypeController  [Route("api/bustype")] ────────────
    public static class BusType
    {
        public const string All = "/api/bustype";
        public const string ById = "/api/bustype/{0}";
        public const string Dropdown = "/api/bustype/dropdown";
    }


    // ── Local DB ──────────────────────────────────────────────────────────
    public static class Database
    {
        public const string Filename = "bustrack.db3";
        public const string EncryptionKey = "BusTrack@2024!SecureKey";
    }

    // ── Cache ─────────────────────────────────────────────────────────────
    public static class Cache
    {
        public const string AppConfig = "app_config_mobile";
        public const string Dashboard = "dashboard_summary";
        public const int AppConfigTtlH = 12;
        public const int DashboardTtlM = 5;
        public const int ListTtlM = 3;
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