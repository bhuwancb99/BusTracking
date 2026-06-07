using BusTracking.Mobile.Helpers;

namespace BusTracking.Mobile;

public partial class AppShell : Shell
{
    private readonly IAuthService _auth;
    private readonly INavigationService _nav;
    int _backPressCounter = 0;
    private static string _svgImageColor = "#ffffff";

    public AppShell(IAuthService auth, INavigationService nav)
    {
        _auth = auth;
        _nav = nav;
        InitializeComponent();
    }

    // ── Called by NavigationService right after login ─────────────────────
    public async Task InitializeForRoleAsync()
    {
        var user = await _auth.GetCurrentUserAsync();
        if (user is null) return;
        _svgImageColor = ResourceColorHelper.GetColor("SvgImageLight", "SvgImageDark");
        LblUserName.Text = user.FullName ?? "User";
        LblRoleLabel.Text = user.Role switch
        {
            Constants.Roles.SuperAdmin => "Super Admin",
            Constants.Roles.BusCoordinator => "Coordinator",
            Constants.Roles.Driver => "Driver",
            Constants.Roles.Parent => "Parent",
            Constants.Roles.Student => "Student",
            _ => user.Role ?? ""
        };

        // Initials for avatar circle
        var parts = (user.FullName ?? "U").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        LblInitials.Text = string.Concat(parts.Take(2).Select(w => char.ToUpper(w[0]).ToString()));

        BuildMenuForRole(user.Role ?? "", user.Permissions ?? "");
    }

    private void BuildMenuForRole(string role, string permissionsJson)
    {
        var items = GetMenuForRole(role, permissionsJson);
        // Mark first item (Dashboard) as active
        if (items.Count > 0) items[0].IsActive = true;
        MenuList.ItemsSource = new ObservableCollection<FlyoutMenuItem>(items);
        MenuList.SelectedItem = null;
    }

    // ── CollectionView tap → navigate and close drawer ────────────────────
    private async void OnMenuItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not FlyoutMenuItem item) return;

        // Update IsActive on all items
        if (MenuList.ItemsSource is ObservableCollection<FlyoutMenuItem> list)
            foreach (var i in list) i.IsActive = (i == item);

        MenuList.SelectedItem = null;
        FlyoutIsPresented = false;

        if (!string.IsNullOrEmpty(item.Route))
            await Shell.Current.GoToAsync("//" + item.Route);
    }

    // ── Menu definitions ──────────────────────────────────────────────────
    private static List<FlyoutMenuItem> GetMenuForRole(string role, string permissionsJson)
        => role switch
        {
            Constants.Roles.SuperAdmin => SuperAdminMenu(),
            Constants.Roles.BusCoordinator => CoordinatorMenu(permissionsJson),
            Constants.Roles.Parent => ParentMenu(),
            Constants.Roles.Student => StudentMenu(),
            Constants.Roles.Driver => DriverMenu(),
            _ => []
        };

    private static List<FlyoutMenuItem> SuperAdminMenu() =>
    [
        new() { IconSvg = "dashboard.png",IconColor=_svgImageColor,   Title = "Dashboard",        Route = "AdminDashboard"       },
        new() { IconSvg = "config.png",IconColor=_svgImageColor,      Title = "App Config",       Route = "AdminConfigList"      },
        new() { IconSvg = "coordinator.png",IconColor=_svgImageColor, Title = "Bus Coordinators", Route = "AdminCoordinatorList" },
        new() { IconSvg = "route.png",IconColor=_svgImageColor,       Title = "Routes",           Route = "AdminRouteList"       },
        new() { IconSvg = "bus.png",IconColor=_svgImageColor,         Title = "Buses",            Route = "AdminBusList"         },
        new() { IconSvg = "driver.png",IconColor=_svgImageColor,      Title = "Drivers",          Route = "AdminDriverList"      },
        new() { IconSvg = "parent.png",IconColor=_svgImageColor,      Title = "Parents",          Route = "AdminParentList"      },
        new() { IconSvg = "student.png",IconColor=_svgImageColor,     Title = "Students",         Route = "AdminStudentList"     },
        new() { IconSvg = "trip.png",IconColor=_svgImageColor,        Title = "Trips",            Route = "AdminTripList"        },
        new() { IconSvg = "notification.png",IconColor=_svgImageColor,Title = "Notifications",    Route = ""                     },
        new() { IconSvg = "help.png",IconColor=_svgImageColor,        Title = "Help & Support",   Route = ""                     },
    ];

    private static List<FlyoutMenuItem> CoordinatorMenu(string permissionsJson)
    {
        HashSet<string> perms = [];
        if (!string.IsNullOrWhiteSpace(permissionsJson))
        {
            try
            {
                var arr = JsonSerializer.Deserialize<List<string>>(permissionsJson);
                if (arr is not null) perms = [.. arr];
            }
            catch { }
        }
        bool Has(string key) => perms.Contains(key);
        // NO fallback — zero permissions = only Dashboard (user sees access-denied if they navigate)

        var menu = new List<FlyoutMenuItem>
        {
            new() { IconSvg = "dashboard.png", IconColor = _svgImageColor, Title = "Dashboard", Route = "CoordinatorDashboard" }
        };

        // ── Same order as SuperAdmin menu ────────────────────────
        if (Has("appconfig.view"))
            menu.Add(new() { IconSvg = "config.png", IconColor = _svgImageColor, Title = "App Config", Route = "CoordConfigList" });

        if (Has("subadmin.view"))
            menu.Add(new() { IconSvg = "coordinator.png", IconColor = _svgImageColor, Title = "Bus Coordinators", Route = "CoordSubAdminList" });

        if (Has("route.view"))
            menu.Add(new() { IconSvg = "route.png", IconColor = _svgImageColor, Title = "Routes", Route = "CoordRouteList" });

        if (Has("bus.view"))
            menu.Add(new() { IconSvg = "bus.png", IconColor = _svgImageColor, Title = "Buses", Route = "CoordBusList" });

        if (Has("driver.view"))
            menu.Add(new() { IconSvg = "driver.png", IconColor = _svgImageColor, Title = "Drivers", Route = "CoordDriverList" });

        if (Has("parent.view"))
            menu.Add(new() { IconSvg = "parent.png", IconColor = _svgImageColor, Title = "Parents", Route = "CoordParentList" });

        if (Has("student.view"))
            menu.Add(new() { IconSvg = "student.png", IconColor = _svgImageColor, Title = "Students", Route = "CoordStudentList" });

        if (Has("trip.view") || Has("trip.manage"))
            menu.Add(new() { IconSvg = "trip.png", IconColor = _svgImageColor, Title = "Trips", Route = "CoordTripList" });

        if (Has("notification.manage"))
            menu.Add(new() { IconSvg = "notification.png", IconColor = _svgImageColor, Title = "Notifications", Route = "CoordNotificationList" });

        if (Has("helpsupport.view") || Has("helpsupport.manage"))
            menu.Add(new() { IconSvg = "feedback.png", IconColor = _svgImageColor, Title = "Help & Support", Route = "CoordFeedbackList" });

        return menu;
    }

    private static List<FlyoutMenuItem> ParentMenu() =>
    [
        new() { IconSvg = "dashboard.png", IconColor = _svgImageColor, Title = "My Dashboard",   Route = "ParentDashboard"  },
        new() { IconSvg = "tracking.png", IconColor = _svgImageColor, Title = "Track Bus",      Route = "ParentTracking"   },
        new() { IconSvg = "availability.png", IconColor = _svgImageColor, Title = "Availability",   Route = "ParentAvailability" },
        new() { IconSvg = "feedback.png", IconColor = _svgImageColor, Title = "Help & Support", Route = "ParentFeedback"   },
    ];

    private static List<FlyoutMenuItem> StudentMenu() =>
    [
        new() { IconSvg = "dashboard.png", IconColor = _svgImageColor, Title = "My Dashboard",    Route = "StudentDashboard"    },
        new() { IconSvg = "tracking.png", IconColor = _svgImageColor, Title = "Track My Bus",    Route = "StudentTracking"     },
        new() { IconSvg = "availability.png", IconColor = _svgImageColor, Title = "My Availability", Route = "StudentAvailability" },
    ];

    private static List<FlyoutMenuItem> DriverMenu() =>
    [
        new() { IconSvg = "dashboard.png", IconColor = _svgImageColor, Title = "Dashboard", Route = "DriverDashboard" },
        new() { IconSvg = "trip.png",      IconColor = _svgImageColor, Title = "My Trips",  Route = "DriverTripList"  },
    ];

    // ── Logout ────────────────────────────────────────────────────────────
    private async void OnLogoutTapped(object? sender, EventArgs e)
    {
        FlyoutIsPresented = false;

        bool confirmed = false;
        if (Application.Current?.Windows[0].Page is Page page)
            confirmed = await page.DisplayAlertAsync(
                "Logout", "Are you sure you want to logout?", "Yes", "No");

        if (!confirmed) return;

        await _auth.LogoutAsync();
        MenuList.ItemsSource = null;
        LblUserName.Text = "";
        LblRoleLabel.Text = "";
        LblInitials.Text = "";

        await _nav.GoToLoginAsync();
    }

    // ── Android back button ───────────────────────────────────────────────
#pragma warning disable CS8602
    protected override bool OnBackButtonPressed()
    {
        try
        {
            if (FlyoutIsPresented) { FlyoutIsPresented = false; return true; }

            if (_backPressCounter >= 2)
            {
#if ANDROID
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#endif
            }
            else if (Navigation.NavigationStack.Count == 1)
            {
                _backPressCounter++;
#if ANDROID
                Android.Widget.Toast.MakeText(
                    Android.App.Application.Context,
                    "Double tap to exit",
                    Android.Widget.ToastLength.Long)?.Show();
#endif
            }
            else { Navigation.PopAsync(); }
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
        return true;
    }
#pragma warning restore CS8602
}
