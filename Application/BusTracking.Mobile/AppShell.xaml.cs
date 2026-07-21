namespace BusTracking.Mobile;

public partial class AppShell : Shell
{
    private readonly IAuthService _auth;
    private readonly INavigationService _nav;
    private readonly IAppConfigService _appConfig;
    int _backPressCounter = 0;
    private static string _svgImageColor = "#ffffff";

    public AppShell(IAuthService auth, INavigationService nav, IAppConfigService appConfig)
    {
        _auth = auth;
        _nav = nav;
        _appConfig = appConfig;
        InitializeComponent();
    }

    // ── Called by NavigationService right after login ─────────────────────
    public async Task InitializeForRoleAsync()
    {
        var user = await _auth.GetCurrentUserAsync();
        if (user is null) return;

        _svgImageColor = ResourceColorHelper.GetColor("SvgImageLight", "SvgImageDark");

        LblUserName.Text = user.FullName ?? "User";
        LblRoleLabel.Text = FriendlyRole(user.Role);

        // Initials for fallback circle
        var parts = (user.FullName ?? "U").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        LblInitials.Text = string.Concat(parts.Take(2).Select(w => char.ToUpper(w[0]).ToString()));

        // Load School Header Card logic:
        // 1. If SchoolName and SchoolLogoUrl are both null/empty, hide BrandSchoolCard.
        // 2. If SchoolName exists:
        //    - Display SchoolName
        //    - If SchoolLogoUrl exists & loads, display logo image
        //    - Else, display initial letter circle (e.g. 'S' for Super Admin or first letter of SchoolName)
        bool hasSchoolName = !string.IsNullOrWhiteSpace(user.SchoolName);
        bool hasSchoolLogo = !string.IsNullOrWhiteSpace(user.SchoolLogoUrl);

        if (!hasSchoolName && !hasSchoolLogo)
        {
            BrandSchoolCard.IsVisible = false;
        }
        else
        {
            BrandSchoolCard.IsVisible = true;

            if (hasSchoolName)
            {
                LblBrandSchoolName.Text = user.SchoolName;
            }
            else
            {
                LblBrandSchoolName.Text = FriendlyRole(user.Role);
            }

            // Determine initial letter (e.g., 'S' for Super Admin or 'D' for Default School)
            char initialChar = 'S';
            if (hasSchoolName && !string.IsNullOrWhiteSpace(user.SchoolName))
            {
                initialChar = char.ToUpper(user.SchoolName.Trim()[0]);
            }
            else if (!string.IsNullOrWhiteSpace(user.Role))
            {
                initialChar = char.ToUpper(user.Role.Trim()[0]);
            }
            LblBrandSchoolInitial.Text = initialChar.ToString();

            string? logoUrl = null;
            if (hasSchoolLogo)
            {
                logoUrl = await ResolveImageUrlAsync(user.SchoolLogoUrl);
            }

            if (!string.IsNullOrWhiteSpace(logoUrl))
            {
                BrandSchoolLogoImage.Source = ImageSource.FromUri(new Uri(logoUrl));
                BrandSchoolLogoBorder.IsVisible = true;
                BrandSchoolInitialBorder.IsVisible = false;
            }
            else
            {
                BrandSchoolLogoBorder.IsVisible = false;
                BrandSchoolInitialBorder.IsVisible = true;
            }
        }

        // Load avatar (respects IsMobileUpdateImage config)
        await LoadAvatarAsync(user.ProfileImageUrl);

        BuildMenuForRole(user.Role ?? "", user.Permissions ?? "");
    }

    // ── Called from ProfilePage after upload or remove ────────────────────
    public async Task RefreshAvatarAsync(string? newStoredUrl)
    {
        await LoadAvatarAsync(newStoredUrl);
    }

    // ── Resolve image URL and show in flyout header ───────────────────────
    /// <summary>
    /// Always extracts only the path from storedUrl and prepends the correct base:
    ///   IsMobileUpdateImage=1 → Constants.ApiBaseUrl   (API server)
    ///   IsMobileUpdateImage=0 → WebsiteImageUrl         (website server)
    ///
    /// This ensures the URL works regardless of which host the API recorded when
    /// saving the image (browser vs emulator vs physical device differ in hostname).
    /// </summary>
    private async Task LoadAvatarAsync(string? storedUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(storedUrl))
            {
                ShowInitials();
                return;
            }

            var resolvedUrl = await ResolveImageUrlAsync(storedUrl);

            if (!string.IsNullOrWhiteSpace(resolvedUrl))
            {
                AvatarImage.Source = ImageSource.FromUri(new Uri(resolvedUrl));
                AvatarImageBorder.IsVisible = true;
                AvatarInitialsBorder.IsVisible = false;
            }
            else
            {
                ShowInitials();
            }
        }
        catch
        {
            ShowInitials();
        }
    }

    /// <summary>
    /// Extracts the AbsolutePath from the stored URL and prepends the correct base URL.
    /// Stored URL example: https://10.0.2.2:7001/media/images/driver/u_5.jpg
    ///   IsMobileUpdateImage=1 → https://10.0.2.2:7001/media/images/driver/u_5.jpg
    ///   IsMobileUpdateImage=0 → https://website.bustracking.com/media/images/driver/u_5.jpg
    /// </summary>
    private async Task<string?> ResolveImageUrlAsync(string? storedUrl)
    {
        if (string.IsNullOrWhiteSpace(storedUrl)) return null;

        // If it's already a full absolute HTTP/HTTPS URL, return it directly
        if (storedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            storedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return storedUrl;
        }

        // Relative path: resolve using configured base URL
        string imagePath = storedUrl.StartsWith('/') ? storedUrl : "/" + storedUrl;
        bool mobileImageEnabled = await _appConfig.IsMobileImageUpdateEnabledAsync();

        if (mobileImageEnabled)
        {
            return Constants.ApiBaseUrl.TrimEnd('/') + imagePath;
        }
        else
        {
            var websiteBase = await _appConfig.GetWebsiteImageUrlAsync();
            return string.IsNullOrWhiteSpace(websiteBase)
                ? Constants.ApiBaseUrl.TrimEnd('/') + imagePath
                : websiteBase.TrimEnd('/') + imagePath;
        }
    }

    private void ShowInitials()
    {
        AvatarImageBorder.IsVisible = false;
        AvatarInitialsBorder.IsVisible = true;
    }

    // ── Build flyout menu ─────────────────────────────────────────────────
    private void BuildMenuForRole(string role, string permissionsJson)
    {
        var items = GetMenuForRole(role, permissionsJson);
        if (items.Count > 0) items[0].IsActive = true;
        MenuList.ItemsSource = new ObservableCollection<FlyoutMenuItem>(items);
        MenuList.SelectedItem = null;
    }

    private async void OnMenuItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not FlyoutMenuItem item) return;

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
        new() { IconSvg = "dashboard.png",    IconColor = _svgImageColor, Title = "Dashboard",        Route = "AdminDashboard"       },
        new() { IconSvg = "profile.png",      IconColor = _svgImageColor, Title = "My Profile",       Route = "Profile"              },
        new() { IconSvg = "config.png",       IconColor = _svgImageColor, Title = "App Config",       Route = "AdminConfigList"      },
        new() { IconSvg = "config.png",       IconColor = _svgImageColor, Title = "Standard Master",  Route = "AdminStandardList"    },
        new() { IconSvg = "bus.png",          IconColor = _svgImageColor, Title = "Bus Types",        Route = "AdminBusTypeList"      },
        new() { IconSvg = "coordinator.png",  IconColor = _svgImageColor, Title = "Bus Coordinators", Route = "AdminCoordinatorList" },
        new() { IconSvg = "route.png",        IconColor = _svgImageColor, Title = "Routes",           Route = "AdminRouteList"       },
        new() { IconSvg = "bus.png",          IconColor = _svgImageColor, Title = "Buses",            Route = "AdminBusList"         },
        new() { IconSvg = "driver.png",       IconColor = _svgImageColor, Title = "Drivers",          Route = "AdminDriverList"      },
        new() { IconSvg = "parent.png",       IconColor = _svgImageColor, Title = "Parents",          Route = "AdminParentList"      },
        new() { IconSvg = "student.png",      IconColor = _svgImageColor, Title = "Students",         Route = "AdminStudentList"     },
        new() { IconSvg = "trip.png",         IconColor = _svgImageColor, Title = "Trips",            Route = "AdminTripList"        },
        new() { IconSvg = "notification.png", IconColor = _svgImageColor, Title = "Notifications",    Route = "AdminNotificationList" },
        new() { IconSvg = "help.png",         IconColor = _svgImageColor, Title = "Help & Support",   Route = "AdminFeedbackList"     },

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

        var menu = new List<FlyoutMenuItem>
        {
            new() { IconSvg = "dashboard.png", IconColor = _svgImageColor, Title = "Dashboard", Route = "CoordinatorDashboard" }
        };
        menu.Add(new() { IconSvg = "profile.png", IconColor = _svgImageColor, Title = "My Profile", Route = "Profile" });

        if (Has("appconfig.view"))
            menu.Add(new() { IconSvg = "config.png", IconColor = _svgImageColor, Title = "App Config", Route = "CoordConfigList" });
        if (Has("student.view"))
            menu.Add(new() { IconSvg = "config.png", IconColor = _svgImageColor, Title = "Standard Master", Route = "CoordStandardList" });
        if (Has("bustype.manage") || Has("bus.view"))
            menu.Add(new() { IconSvg = "bus.png", IconColor = _svgImageColor, Title = "Bus Types", Route = "CoordBusTypeList" });
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
        new() { IconSvg = "dashboard.png",    IconColor = _svgImageColor, Title = "My Dashboard",   Route = "ParentDashboard"    },
        new() { IconSvg = "profile.png",      IconColor = _svgImageColor, Title = "My Profile",     Route = "Profile"            },
        new() { IconSvg = "tracking.png",     IconColor = _svgImageColor, Title = "Track Bus",      Route = "ParentTracking"     },
        new() { IconSvg = "availability.png", IconColor = _svgImageColor, Title = "Availability",   Route = "ParentAvailability" },
        new() { IconSvg = "feedback.png",     IconColor = _svgImageColor, Title = "Help & Support", Route = "ParentFeedback"     },
    ];

    private static List<FlyoutMenuItem> StudentMenu() =>
    [
        new() { IconSvg = "dashboard.png",    IconColor = _svgImageColor, Title = "My Dashboard",    Route = "StudentDashboard"    },
        new() { IconSvg = "profile.png",      IconColor = _svgImageColor, Title = "My Profile",      Route = "Profile"             },
        new() { IconSvg = "tracking.png",     IconColor = _svgImageColor, Title = "Track My Bus",    Route = "StudentTracking"     },
        new() { IconSvg = "availability.png", IconColor = _svgImageColor, Title = "My Availability", Route = "StudentAvailability" },
    ];

    private static List<FlyoutMenuItem> DriverMenu() =>
    [
        new() { IconSvg = "dashboard.png",      IconColor = _svgImageColor,     Title = "Dashboard",        Route = "DriverDashboard" },
        new() { IconSvg = "profile.png",        IconColor = _svgImageColor,     Title = "My Profile",       Route = "Profile"         },
        new() { IconSvg = "trip.png",           IconColor = _svgImageColor,     Title = "My Trips",         Route = "DriverTripList"  },
        new() { IconSvg = "notification.png",   IconColor = _svgImageColor,     Title = "Notifications",    Route = "DriverNotification"   },
    ];

    // ── Logout ────────────────────────────────────────────────────────────
    private async void OnLogoutTapped(object? sender, EventArgs e)
    {
        FlyoutIsPresented = false;

        bool confirmed = false;
        if (Application.Current?.Windows[0].Page is Page page)
        {
            var popup = new Views.Common.ConfirmPopup(
                "Logout",
                "Are you sure you want to logout?",
                "Yes, Logout",
                "Cancel",
                "logout.png",
                Color.FromArgb("#ba1a1a")
            );
            var result = await page.ShowPopupAsync<bool>(popup);
            confirmed = result is not null && result.Result;
        }

        if (!confirmed) return;

        await _auth.LogoutAsync();
        MenuList.ItemsSource = null;
        LblUserName.Text = "";
        LblRoleLabel.Text = "";
        LblInitials.Text = "";
        ShowInitials();

        await _nav.GoToLoginAsync();
    }

    // ── Helper ────────────────────────────────────────────────────────────
    private static string FriendlyRole(string? role) => role switch
    {
        Constants.Roles.SuperAdmin => "Super Admin",
        Constants.Roles.BusCoordinator => "Coordinator",
        Constants.Roles.Driver => "Driver",
        Constants.Roles.Parent => "Parent",
        Constants.Roles.Student => "Student",
        _ => role ?? ""
    };

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
            else
            {
                Navigation.PopAsync();
            }
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
        return true;
    }
#pragma warning restore CS8602
}
