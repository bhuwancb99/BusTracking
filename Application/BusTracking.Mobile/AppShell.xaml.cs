using System.Collections.ObjectModel;
using System.Text.Json;

namespace BusTracking.Mobile;

// Simple model for each flyout row
public class FlyoutMenuItem
{
    public string Icon { get; init; } = "";
    public string Title { get; init; } = "";
    public string Route { get; init; } = "";
}

public partial class AppShell : Shell
{
    private readonly IAuthService _auth;
    private readonly INavigationService _nav;
    int _backPressCounter = 0;

    // ─────────────────────────────────────────────────────────────────────
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

        // Update name + role badge labels (named elements in XAML)
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

        BuildMenuForRole(user.Role ?? "", user.Permissions ?? "");
    }

    // ── Populate the CollectionView with the correct menu items ───────────
    private void BuildMenuForRole(string role, string permissionsJson)
    {
        var items = GetMenuForRole(role, permissionsJson);
        MenuList.ItemsSource = new ObservableCollection<FlyoutMenuItem>(items);
        MenuList.SelectedItem = null;   // no item highlighted yet
    }

    // ── CollectionView tap → navigate and close drawer ────────────────────
    private async void OnMenuItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not FlyoutMenuItem item
            || string.IsNullOrEmpty(item.Route))
            return;

        // Deselect immediately so tapping the same item again works
        MenuList.SelectedItem = null;
        FlyoutIsPresented = false;

        await Shell.Current.GoToAsync("//" + item.Route);
    }

    // ── Menu definitions — mirrors web SidebarHelper exactly ─────────────
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

    // SuperAdmin — full menu, no permission gate
    private static List<FlyoutMenuItem> SuperAdminMenu() =>
    [
        new() { Icon = "🏠",  Title = "Dashboard",        Route = "AdminDashboard"      },
        new() { Icon = "⚙️",  Title = "App Config",       Route = "AdminConfigList"     },
        new() { Icon = "📋",  Title = "Bus Coordinators", Route = "AdminCoordinatorList"},
        new() { Icon = "🗺️", Title = "Routes",            Route = "AdminRouteList"      },
        new() { Icon = "🚌",  Title = "Buses",            Route = "AdminBusList"        },
        new() { Icon = "🧑‍✈️",Title = "Drivers",           Route = "AdminDriverList"     },
        new() { Icon = "👨‍👩‍👧",Title = "Parents",          Route = "AdminParentList"     },
        new() { Icon = "🎒",  Title = "Students",         Route = "AdminStudentList"    },
        new() { Icon = "🚐",  Title = "Trips",            Route = "AdminTripList"       },
    ];

    // Coordinator — permission-filtered, mirrors web CoordinatorMenu()
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
            catch { /* malformed — treat as empty */ }
        }

        bool Has(string key) => perms.Contains(key);
        bool fallback = perms.Count == 0; // old session: show everything

        var menu = new List<FlyoutMenuItem>
        {
            new() { Icon = "🏠", Title = "Dashboard", Route = "CoordinatorDashboard" }
        };

        if (Has("route.view") || fallback)
            menu.Add(new() { Icon = "🗺️", Title = "Routes", Route = "CoordRouteList" });
        if (Has("bus.view") || fallback)
            menu.Add(new() { Icon = "🚌", Title = "Buses", Route = "CoordBusList" });
        if (Has("driver.view") || fallback)
            menu.Add(new() { Icon = "🧑‍✈️", Title = "Drivers", Route = "CoordDriverList" });
        if (Has("parent.view") || fallback)
            menu.Add(new() { Icon = "👨‍👩‍👧", Title = "Parents", Route = "CoordParentList" });
        if (Has("student.view") || fallback)
            menu.Add(new() { Icon = "🎒", Title = "Students", Route = "CoordStudentList" });
        if (Has("trip.view") || Has("trip.manage") || fallback)
            menu.Add(new() { Icon = "🚐", Title = "Trips", Route = "CoordTripList" });

        return menu;
    }

    // Parent — mirrors web ParentMenu()
    private static List<FlyoutMenuItem> ParentMenu() =>
    [
        new() { Icon = "🏠", Title = "Dashboard",  Route = "ParentDashboard" },
        new() { Icon = "📍", Title = "Track Bus",  Route = "ParentTracking"  },
    ];

    // Student — mirrors web StudentMenu()
    private static List<FlyoutMenuItem> StudentMenu() =>
    [
        new() { Icon = "🏠", Title = "Dashboard",      Route = "StudentDashboard"   },
        new() { Icon = "📍", Title = "Track My Bus",   Route = "StudentTracking"    },
        new() { Icon = "📅", Title = "My Availability",Route = "StudentAvailability"},
    ];

    // Driver
    private static List<FlyoutMenuItem> DriverMenu() =>
    [
        new() { Icon = "🏠", Title = "Dashboard", Route = "DriverDashboard" },
        new() { Icon = "📋", Title = "My Trips",  Route = "DriverTripList"  },
    ];

    // ── Logout ────────────────────────────────────────────────────────────
    private async void OnLogoutTapped(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;

        bool confirmed = false;
        if (Application.Current?.Windows[0].Page is Page page)
            confirmed = await page.DisplayAlertAsync(
                "Logout", "Are you sure you want to logout?", "Yes", "No");

        if (!confirmed) return;

        await _auth.LogoutAsync();

        // Clear menu for next user
        MenuList.ItemsSource = null;
        LblUserName.Text = "";
        LblRoleLabel.Text = "";

        await _nav.GoToLoginAsync();
    }

    // ── Android back button ───────────────────────────────────────────────
#pragma warning disable CS8602
    protected override bool OnBackButtonPressed()
    {
        try
        {
            if (FlyoutIsPresented)
            {
                FlyoutIsPresented = false;
                return true;
            }

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
            else
            {
                Navigation.PopAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return true;
    }
#pragma warning restore CS8602
}