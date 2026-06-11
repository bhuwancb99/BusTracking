namespace BusTracking.Mobile.Viewmodels.Common
{
    /// <summary>
    /// Image URL resolution logic:
    ///   IsMobileUpdateImage = 1  →  upload/remove enabled
    ///                               display URL = Constants.ApiBaseUrl + path(ProfileImageUrl)
    ///   IsMobileUpdateImage = 0  →  read-only display only
    ///                               display URL = WebsiteImageUrl + path(ProfileImageUrl)
    ///
    /// We always strip the host from the stored URL and prepend the correct base because
    /// the API's ImageService records the URL from the live request host, which differs
    /// between browser (localhost), Android emulator (10.0.2.2), and physical devices.
    /// </summary>
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly IAppConfigService _appConfig;
        private readonly IApiService _api;


        [ObservableProperty] private string _fullName = "";
        [ObservableProperty] private string _email = "";
        [ObservableProperty] private string _roleDisplay = "";
        [ObservableProperty] private string _initials = "";

        /// <summary>
        /// Resolved image URL ready for binding to Image.Source via UriImageSource.
        /// Null = show initials circle instead.
        /// </summary>
        [ObservableProperty] private string? _profileImageUrl;

        /// <summary>True when a photo is currently displayed (controls Remove button visibility).</summary>
        [ObservableProperty] private bool _hasPhoto;

        /// <summary>
        /// True when IsMobileUpdateImage = 1.
        /// Controls visibility of Upload and Remove buttons via binding.
        /// </summary>
        [ObservableProperty] private bool _canUpdateImage;

        /// <summary>
        /// Upload button label — "Upload Photo" when no photo, "Change Photo" when one exists.
        /// </summary>
        [ObservableProperty] private string _uploadButtonText = "Upload Photo";

        // Raw stored URL from DB/session (host may differ — always resolve before display)
        private string? _rawStoredUrl;
        private SessionUser? _currentUser;

        // ── Constructor ───────────────────────────────────────────────────────────

        public ProfileViewModel(
            IAuthService auth,
            INavigationService nav,
            IAppConfigService appConfig,
            IApiService api)
            : base(auth, nav)
        {
            Title = "My Profile";
            _appConfig = appConfig;
            _api = api;
        }

        // ── InitializeAsync (called once by ViewBase on first appearance) ─────────

        public override async Task InitializeAsync()
        {
            await RunAsync(LoadProfileAsync);
        }

        // ── RefreshOnReturnAsync (called by ViewBase on re-appearance) ────────────

        public override async Task RefreshOnReturnAsync()
        {
            // Re-resolve image URL in case config changed while away
            await RunAsync(async () =>
            {
                CanUpdateImage = await _appConfig.IsMobileImageUpdateEnabledAsync();
                await ResolveAndSetImageAsync(_rawStoredUrl);
                UpdateButtonState();
            });
        }

        // ── Core load ─────────────────────────────────────────────────────────────

        private async Task LoadProfileAsync()
        {
            _currentUser = await Auth.GetCurrentUserAsync();
            if (_currentUser is null) return;

            // Text properties
            FullName = _currentUser.FullName ?? "";
            Email = _currentUser.Email ?? "";
            RoleDisplay = FriendlyRole(_currentUser.Role);

            // Initials (up to 2 words, first letter each)
            var parts = (_currentUser.FullName ?? "U")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Initials = string.Concat(parts.Take(2).Select(w => char.ToUpper(w[0]).ToString()));

            // Config flag
            CanUpdateImage = await _appConfig.IsMobileImageUpdateEnabledAsync();

            // Store raw URL and resolve for display
            _rawStoredUrl = _currentUser.ProfileImageUrl;
            await ResolveAndSetImageAsync(_rawStoredUrl);

            UpdateButtonState();
        }

        // ── Image URL resolution ──────────────────────────────────────────────────

        /// <summary>
        /// Resolves storedUrl to a displayable URL and sets ProfileImageUrl + HasPhoto.
        /// Sets null (show initials) if URL cannot be resolved.
        /// </summary>
        private async Task ResolveAndSetImageAsync(string? storedUrl)
        {
            var resolved = await ResolveImageUrlAsync(storedUrl);
            ProfileImageUrl = resolved;
            HasPhoto = !string.IsNullOrWhiteSpace(resolved) && !string.IsNullOrWhiteSpace(storedUrl);
        }

        /// <summary>
        /// Extracts only the path portion from the stored URL and prepends the
        /// correct base URL based on IsMobileUpdateImage config value.
        ///
        /// Stored example : https://10.0.2.2:7001/media/images/driver/u_5.jpg
        ///   flag = 1 → https://10.0.2.2:7001/media/images/driver/u_5.jpg   (from ApiBaseUrl)
        ///   flag = 0 → https://website.com/media/images/driver/u_5.jpg      (from WebsiteImageUrl)
        /// </summary>
        private async Task<string?> ResolveImageUrlAsync(string? storedUrl)
        {
            if (string.IsNullOrWhiteSpace(storedUrl)) return null;

            // Extract path only — e.g. "/media/images/driver/u_5.jpg"
            string imagePath;
            try
            {
                imagePath = new Uri(storedUrl).AbsolutePath;
            }
            catch
            {
                // storedUrl is already a relative path
                imagePath = storedUrl.StartsWith('/') ? storedUrl : "/" + storedUrl;
            }

            if (CanUpdateImage)
            {
                // IsMobileUpdateImage = 1 → serve from API server
                return Constants.ApiBaseUrl.TrimEnd('/') + imagePath;
            }
            else
            {
                // IsMobileUpdateImage = 0 → serve from website server
                var websiteBase = await _appConfig.GetWebsiteImageUrlAsync();
                if (string.IsNullOrWhiteSpace(websiteBase)) return null;
                return websiteBase.TrimEnd('/') + imagePath;
            }
        }

        // ── Button state ──────────────────────────────────────────────────────────

        private void UpdateButtonState()
        {
            if (!CanUpdateImage)
            {
                CanUpdateImage = false;
                UploadButtonText = "Upload Photo";
                HasPhoto = false;
                return;
            }

            bool hasRaw = !string.IsNullOrWhiteSpace(_rawStoredUrl);
            HasPhoto = hasRaw;
            UploadButtonText = hasRaw ? "Change Photo" : "Upload Photo";
        }

        // ── Commands ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Pick a photo using .NET 10 PickPhotosAsync (single selection).
        /// Uploads to POST /api/profile/photo via multipart/form-data.
        /// Updates session, ProfileImageUrl, and notifies AppShell to refresh flyout avatar.
        /// </summary>
        [RelayCommand]
        private async Task UploadPhotoAsync()
        {
            if (!CanUpdateImage) return;

            IEnumerable<FileResult>? photos = null;
            photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                Title = "Select profile photo",
                SelectionLimit = 1,
                MaximumWidth = 1024,
                MaximumHeight = 768,
                CompressionQuality = 85,
                RotateImage = true,
                PreserveMetaData = true
            });

            if (photos is null) return;
            var photo = photos.FirstOrDefault();
            if (photo is null) return;

            await RunAsync(async () =>
            {
                using var stream = await photo.OpenReadAsync();
                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(
                        photo.ContentType ?? "image/jpeg");
                content.Add(streamContent, "file", photo.FileName);

                // POST api/profile/photo — returns full stored URL (API server host)
                var r = await _api.PostMultipartAsync<string>(
                    Constants.Common.ProfilePhoto, content);

                if (r.Success && r.Data is not null)
                {
                    // Store raw URL in session (host will be re-resolved at display time)
                    _rawStoredUrl = r.Data;
                    await Auth.RefreshProfileImageAsync(r.Data);

                    if (_currentUser is not null)
                        _currentUser.ProfileImageUrl = r.Data;

                    await ResolveAndSetImageAsync(r.Data);
                    UpdateButtonState();

                    // Refresh flyout avatar in AppShell
                    if (Shell.Current is AppShell shell)
                        await shell.RefreshAvatarAsync(r.Data);

                    await ShowToastAsync("Profile photo updated.");
                }
                else
                {
                    await ShowAlertAsync("Error", r.Message ?? "Upload failed. Please try again.");
                }
            });
        }

        /// <summary>
        /// Confirm and remove profile photo via DELETE /api/profile/photo.
        /// Clears session URL, resets to initials, refreshes flyout avatar.
        /// </summary>
        [RelayCommand]
        private async Task RemovePhotoAsync()
        {
            if (!CanUpdateImage) return;

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
                    UpdateButtonState();

                    // Refresh flyout back to initials
                    if (Shell.Current is AppShell shell)
                        await shell.RefreshAvatarAsync(null);

                    await ShowToastAsync("Profile photo removed.");
                }
                else
                {
                    await ShowAlertAsync("Error", r.Message ?? "Remove failed. Please try again.");
                }
            });
        }

        // ── Helper ────────────────────────────────────────────────────────────────

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
