namespace BusTracking.Web.Helpers;

// ─── Sidebar menu builder ─────────────────────────────────────────────
public static class SidebarHelper
{
    public static List<SidebarMenuItem> GetMenu(ClaimsPrincipal user)
    {
        var role = user.GetRole();

        return role switch
        {
            AppConstants.RoleSuperAdmin => SuperAdminMenu(),
            AppConstants.RoleBusCoordinator => CoordinatorMenu(user),
            AppConstants.RoleParent => ParentMenu(),
            AppConstants.RoleStudent => StudentMenu(),
            _ => []
        };
    }

    private static List<SidebarMenuItem> SuperAdminMenu() =>
    [
        new() { Label = "Dashboard",       Icon = "bi-speedometer2",  Controller = "Dashboard", Action = "Index", Area = "SuperAdmin" },
        new() { Label = "Bus Coordinators",Icon = "bi-person-badge",  Controller = "SubAdmin",  Action = "Index", Area = "SuperAdmin" },
        new() { Label = "Routes",          Icon = "bi-map",           Controller = "Route",     Action = "Index", Area = "SuperAdmin" },
        new() { Label = "Buses",           Icon = "bi-bus-front",     Controller = "Bus",       Action = "Index", Area = "SuperAdmin" },
        new() { Label = "Drivers",         Icon = "bi-person-video2", Controller = "Driver",    Action = "Index", Area = "SuperAdmin" },
        new() { Label = "Parents",         Icon = "bi-people",        Controller = "Parent",    Action = "Index", Area = "SuperAdmin" },
        new() { Label = "Students",        Icon = "bi-mortarboard",   Controller = "Student",   Action = "Index", Area = "SuperAdmin" },
        new() { Label = "Trips",           Icon = "bi-signpost-split",Controller = "Trip",      Action = "Index", Area = "SuperAdmin" },
        new() { Label = "Help & Support",  Icon = "bi-headset",       Controller = "Feedback",  Action = "Index", Area = "SuperAdmin" },
    ];

    private static List<SidebarMenuItem> CoordinatorMenu(ClaimsPrincipal user)
    {
        var perms = user.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToHashSet();

        bool Has(string key) => perms.Contains(key);
        // If no permission claims in cookie (old session), show everything
        bool fallback = !perms.Any();

        var menu = new List<SidebarMenuItem>
        {
            new() { Label = "Dashboard", Icon = "bi-speedometer2", Controller = "Dashboard", Action = "Index", Area = "BusCoordinator" }
        };

        if (Has("route.view") || fallback) menu.Add(new() { Label = "Routes", Icon = "bi-map", Controller = "Route", Action = "Index", Area = "BusCoordinator" });
        if (Has("bus.view") || fallback) menu.Add(new() { Label = "Buses", Icon = "bi-bus-front", Controller = "Bus", Action = "Index", Area = "BusCoordinator" });
        if (Has("driver.view") || fallback) menu.Add(new() { Label = "Drivers", Icon = "bi-person-video2", Controller = "Driver", Action = "Index", Area = "BusCoordinator" });
        if (Has("parent.view") || fallback) menu.Add(new() { Label = "Parents", Icon = "bi-people", Controller = "Parent", Action = "Index", Area = "BusCoordinator" });
        if (Has("student.view") || fallback) menu.Add(new() { Label = "Students", Icon = "bi-mortarboard", Controller = "Student", Action = "Index", Area = "BusCoordinator" });
        if (Has("trip.view") || Has("trip.manage") || fallback)
            menu.Add(new() { Label = "Trips", Icon = "bi-signpost-split", Controller = "Trip", Action = "Index", Area = "BusCoordinator" });
        if (Has("notification.manage") || fallback)
            menu.Add(new() { Label = "Notifications", Icon = "bi-bell", Controller = "Notification", Action = "Index", Area = "BusCoordinator" });
        if (Has("helpsupport.view") || Has("helpsupport.manage") || fallback)
            menu.Add(new() { Label = "Support", Icon = "bi-headset", Controller = "Feedback", Action = "Index", Area = "BusCoordinator" });

        return menu;
    }

    private static List<SidebarMenuItem> ParentMenu() =>
    [
        new() { Label = "Dashboard",     Icon = "bi-speedometer2",   Controller = "Dashboard",    Action = "Index",        Area = "Parent" },
        new() { Label = "Track Bus",     Icon = "bi-geo-alt",        Controller = "Tracking",     Action = "Track",        Area = "Parent" },
        new() { Label = "Availability",  Icon = "bi-calendar-check", Controller = "Student",      Action = "Availability", Area = "Parent" },
        new() { Label = "Notifications", Icon = "bi-bell",           Controller = "Notification", Action = "Index",        Area = "Parent" },
        new() { Label = "Help & Support",Icon = "bi-headset",        Controller = "Feedback",     Action = "Submit",       Area = "Parent" },
    ];

    private static List<SidebarMenuItem> StudentMenu() =>
    [
        new() { Label = "Dashboard",      Icon = "bi-speedometer2",   Controller = "Dashboard",    Action = "Index",        Area = "Student" },
        new() { Label = "Track My Bus",   Icon = "bi-geo-alt",        Controller = "Tracking",     Action = "Track",        Area = "Student" },
        new() { Label = "My Availability",Icon = "bi-calendar-check", Controller = "Student",      Action = "Availability", Area = "Student" },
        new() { Label = "Notifications",  Icon = "bi-bell",           Controller = "Notification", Action = "Index",        Area = "Student" },
        new() { Label = "Help & Support", Icon = "bi-headset",        Controller = "Feedback",     Action = "Submit",       Area = "Student" },
    ];
}