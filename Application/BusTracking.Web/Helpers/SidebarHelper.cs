using BusTracking.Common.Helpers;
using BusTracking.Common.Models;
using System.Security.Claims;

namespace BusTracking.Web.Helpers
{
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
            new() { Label = "Dashboard",      Icon = "bi-speedometer2",   Controller = "Dashboard",  Action = "Index" },
        new() { Label = "Bus Coordinators",Icon = "bi-person-badge",  Controller = "SubAdmin",   Action = "Index" },
        new() { Label = "Routes",         Icon = "bi-map",            Controller = "Route",      Action = "Index" },
        new() { Label = "Buses",          Icon = "bi-bus-front",      Controller = "Bus",        Action = "Index" },
        new() { Label = "Drivers",        Icon = "bi-person-video2",  Controller = "Driver",     Action = "Index" },
        new() { Label = "Parents",        Icon = "bi-people",         Controller = "Parent",     Action = "Index" },
        new() { Label = "Students",       Icon = "bi-mortarboard",    Controller = "Student",    Action = "Index" },
        new() { Label = "Help & Support", Icon = "bi-headset",        Controller = "Feedback",   Action = "Index" },
    ];

        private static List<SidebarMenuItem> CoordinatorMenu(ClaimsPrincipal user)
        {
            // In a real app you'd check SubAdminPermissions claims here
            // For simplicity, coordinators see same as admin minus SubAdmin management
            return
            [
                new() { Label = "Dashboard",  Icon = "bi-speedometer2",  Controller = "Dashboard", Action = "Index" },
            new() { Label = "Routes",     Icon = "bi-map",           Controller = "Route",     Action = "Index" },
            new() { Label = "Buses",      Icon = "bi-bus-front",     Controller = "Bus",       Action = "Index" },
            new() { Label = "Drivers",    Icon = "bi-person-video2", Controller = "Driver",    Action = "Index" },
            new() { Label = "Parents",    Icon = "bi-people",        Controller = "Parent",    Action = "Index" },
            new() { Label = "Students",   Icon = "bi-mortarboard",   Controller = "Student",   Action = "Index" },
            new() { Label = "Support",    Icon = "bi-headset",       Controller = "Feedback",  Action = "Index" },
        ];
        }

        private static List<SidebarMenuItem> ParentMenu() =>
        [
            new() { Label = "Dashboard",     Icon = "bi-speedometer2", Controller = "Dashboard",    Action = "Index" },
        new() { Label = "Track Bus",     Icon = "bi-geo-alt",      Controller = "Tracking",     Action = "Track" },
        new() { Label = "Availability",  Icon = "bi-calendar-check",Controller = "Student",     Action = "Availability" },
        new() { Label = "Notifications", Icon = "bi-bell",         Controller = "Notification", Action = "Index" },
        new() { Label = "Help & Support",Icon = "bi-headset",      Controller = "Feedback",     Action = "Submit" },
    ];

        private static List<SidebarMenuItem> StudentMenu() =>
        [
            new() { Label = "Dashboard",     Icon = "bi-speedometer2", Controller = "Dashboard",    Action = "Index" },
        new() { Label = "Track My Bus",  Icon = "bi-geo-alt",      Controller = "Tracking",     Action = "Track" },
        new() { Label = "My Availability",Icon = "bi-calendar-check",Controller = "Student",    Action = "Availability" },
        new() { Label = "Notifications", Icon = "bi-bell",         Controller = "Notification", Action = "Index" },
        new() { Label = "Help & Support",Icon = "bi-headset",      Controller = "Feedback",     Action = "Submit" },
    ];
    }
}
