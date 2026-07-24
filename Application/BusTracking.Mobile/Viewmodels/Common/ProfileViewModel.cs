namespace BusTracking.Mobile.Viewmodels.Common
{
    /// <summary>
    /// Photo update entry point: camera badge on avatar only.
    ///   → ActionSheet: "Take Photo" (camera) | "Choose from Gallery" (picker)
    ///   → Both paths share UploadFileAsync → POST /api/profile/photo
    ///
    /// Image URL resolution:
    ///   IsMobileUpdateImage = 1  →  camera badge + Remove button enabled
    ///                               display URL = Constants.ApiBaseUrl + path
    ///   IsMobileUpdateImage = 0  →  camera badge + Remove button hidden (read-only)
    ///                               display URL = WebsiteImageUrl + path
    /// </summary>
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly IAppConfigService _appConfig;
        private readonly IApiService _api;

        // ── Observable properties ──────────────────────────────────────────────

        [ObservableProperty] private string _fullName = "";
        [ObservableProperty] private string _email = "";
        [ObservableProperty] private string _roleDisplay = "";

        /// <summary>
        /// Up to 2-letter initials shown inside the coloured circle when no photo exists.
        /// e.g. "Super Admin" → "SA",  "John Doe" → "JD",  "Mary" → "M"
        /// </summary>
        [ObservableProperty] private string _initials = "";

        /// <summary>
        /// Fully resolved display URL for UriImageSource binding.
        /// null/empty → show initials circle.   non-null → show photo circle.
        /// </summary>
        [ObservableProperty] private string? _profileImageUrl;

        /// <summary>
        /// True when a photo is stored and resolved.
        /// Drives Remove button visibility (combined with CanUpdateImage via AllTrueConverter).
        /// </summary>
        [ObservableProperty] private bool _hasPhoto;

        /// <summary>
        /// True when IsMobileUpdateImage = "1".
        /// Controls camera badge and Remove button visibility.
        /// </summary>
        [ObservableProperty] private bool _canUpdateImage;

        // Private backing fields
        private string? _rawStoredUrl;
        private bool _mobileImageEnabled;
        private SessionUser? _currentUser;

        // ── Constructor ────────────────────────────────────────────────────────

        public ProfileViewModel(
            IAuthService auth,
            INavigationService nav,
            IAppConfigService appConfig,
            IApiService api) : base(auth, nav)
        {
            Title = "My Profile";
            _appConfig = appConfig;
            _api = api;
        }

        // ── ViewBase hooks ─────────────────────────────────────────────────────

        public override async Task InitializeAsync()
            => await RunAsync(LoadProfileAsync);

        public override async Task RefreshOnReturnAsync()
        {
            await RunAsync(async () =>
            {
                _mobileImageEnabled = await _appConfig.IsMobileImageUpdateEnabledAsync();
                CanUpdateImage = _mobileImageEnabled;
                await ResolveAndSetImageAsync(_rawStoredUrl);
                UpdateHasPhoto();
            });
        }

        // ── Core load ──────────────────────────────────────────────────────────

        private async Task LoadProfileAsync()
        {
            _currentUser = await Auth.GetCurrentUserAsync();
            if (_currentUser is null) return;

            FullName = _currentUser.FullName ?? "";
            Email = _currentUser.Email ?? "";
            RoleDisplay = FriendlyRole(_currentUser.Role);

            // Initials — first letter of each word, max 2
            var words = (_currentUser.FullName ?? "U")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Initials = string.Concat(
                words.Take(2).Select(w => char.ToUpper(w[0]).ToString()));

            _mobileImageEnabled = await _appConfig.IsMobileImageUpdateEnabledAsync();
            CanUpdateImage = _mobileImageEnabled;

            _rawStoredUrl = _currentUser.ProfileImageUrl;
            await ResolveAndSetImageAsync(_rawStoredUrl);
            UpdateHasPhoto();
        }

        // ── Image resolution ───────────────────────────────────────────────────

        private async Task ResolveAndSetImageAsync(string? storedUrl)
        {
            ProfileImageUrl = await ResolveImageUrlAsync(storedUrl);
            UpdateHasPhoto();
        }

        /// <summary>
        /// Strips the host from the stored URL and prepends the correct base:
        ///   flag=1 → Constants.ApiBaseUrl + path   (API server)
        ///   flag=0 → WebsiteImageUrl       + path   (website server)
        /// </summary>
        private async Task<string?> ResolveImageUrlAsync(string? storedUrl)
        {
            if (string.IsNullOrWhiteSpace(storedUrl)) return null;

            if (storedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                storedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return storedUrl;
            }

            string imagePath = storedUrl.StartsWith('/') ? storedUrl : "/" + storedUrl;

            if (_mobileImageEnabled)
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

        private void UpdateHasPhoto()
            => HasPhoto = !string.IsNullOrWhiteSpace(ProfileImageUrl)
                       && !string.IsNullOrWhiteSpace(_rawStoredUrl);

        // ── Shared upload ──────────────────────────────────────────────────────

        private async Task UploadFileAsync(FileResult photo)
        {
            await RunAsync(async () =>
            {
                using var stream = await photo.OpenReadAsync();
                using var formContent = new MultipartFormDataContent();
                var sc = new StreamContent(stream);
                sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    photo.ContentType ?? "image/jpeg");
                formContent.Add(sc, "file", photo.FileName);

                var r = await _api.PostMultipartAsync<string>(
                    Constants.Common.ProfilePhoto, formContent);

                if (r.Success && r.Data is not null)
                {
                    _rawStoredUrl = r.Data;
                    await Auth.RefreshProfileImageAsync(r.Data);
                    if (_currentUser is not null)
                        _currentUser.ProfileImageUrl = r.Data;

                    await ResolveAndSetImageAsync(r.Data);

                    if (Shell.Current is AppShell shell)
                        await shell.RefreshAvatarAsync(r.Data);

                    await ShowToastAsync("Profile photo updated.");
                }
                else
                {
                    await ShowAlertAsync("Error",
                        r.Message ?? "Upload failed. Please try again.");
                }
            });
        }

        // ── Commands ───────────────────────────────────────────────────────────

        /// <summary>
        /// Camera badge tap → ActionSheet with two options:
        ///   "Take Photo"          → CapturePhotoAsync  (live camera)
        ///   "Choose from Gallery" → PickPhotosAsync    (.NET 10, single)
        ///
        /// Visible only when IsMobileUpdateImage = 1  (CanUpdateImage = true).
        /// </summary>
        [RelayCommand]
        private async Task CaptureOrPickPhotoAsync()
        {
            if (!_mobileImageEnabled) return;

            string? action = await Shell.Current.DisplayActionSheetAsync(
                "Update Profile Photo",
                "Cancel",
                null,
                "Take Photo",
                "Choose from Gallery");

            if (action is null or "Cancel") return;

            FileResult? photo = null;

            if (action == "Take Photo")
            {
                try
                {
                    photo = await MediaPicker.Default.CapturePhotoAsync();
                }
                catch (FeatureNotSupportedException)
                {
                    await ShowAlertAsync("Not Supported",
                        "Camera capture is not supported on this device.");
                    return;
                }
                catch (PermissionException)
                {
                    await ShowAlertAsync("Permission Required",
                        "Camera permission is required to take a photo.");
                    return;
                }
            }
            else // Choose from Gallery
            {
                var results = await MediaPicker.Default.PickPhotosAsync(
                    new MediaPickerOptions
                    {
                        Title = "Select profile photo",
                        SelectionLimit = 1,
                        MaximumWidth = 1024,
                        MaximumHeight = 768,
                        CompressionQuality = 85,
                        RotateImage = true,
                        PreserveMetaData = true
                    });
                photo = results?.FirstOrDefault();
            }

            if (photo is null) return;
            await UploadFileAsync(photo);
        }

        /// <summary>
        /// Remove photo → DELETE /api/profile/photo.
        /// Visible only when CanUpdateImage = true AND HasPhoto = true.
        /// </summary>
        [RelayCommand]
        private async Task RemovePhotoAsync()
        {
            if (!_mobileImageEnabled) return;

            bool confirmed = await ConfirmAsync(
                "Remove Photo",
                "Are you sure you want to remove your profile photo?",
                "Yes, Remove", "Cancel");
            if (!confirmed) return;

            await RunAsync(async () =>
            {
                var r = await _api.DeleteAsync<bool>(Constants.Common.ProfilePhoto);

                if (r.Success)
                {
                    _rawStoredUrl = null;
                    await Auth.RefreshProfileImageAsync(null);
                    if (_currentUser is not null)
                        _currentUser.ProfileImageUrl = null;

                    ProfileImageUrl = null;
                    UpdateHasPhoto();

                    if (Shell.Current is AppShell shell)
                        await shell.RefreshAvatarAsync(null);

                    await ShowToastAsync("Profile photo removed.");
                }
                else
                {
                    await ShowAlertAsync("Error",
                        r.Message ?? "Remove failed. Please try again.");
                }
            });
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
    }
}
