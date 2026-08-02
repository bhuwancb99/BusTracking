namespace BusTracking.Mobile.Views.Common;

/// <summary>
/// Custom popup for app update prompts.
/// Result: "update" if user taps Update Now, "later" if user taps Later.
/// Mandatory mode hides the Later button — popup can only be dismissed by tapping Update Now.
/// </summary>
public partial class UpdatePopup : CommunityToolkit.Maui.Views.Popup
{
    private readonly string _updateUrl;
    private readonly bool _isMandatory;

    public UpdatePopup(string currentVersion, string newVersion, string updateUrl, bool isMandatory)
    {
        InitializeComponent();
        _updateUrl = updateUrl;
        _isMandatory = isMandatory;

        LblCurrentVersion.Text = currentVersion;
        LblNewVersion.Text = newVersion;

        if (isMandatory)
        {
            // Mandatory update: hide Later button, make Update button span full width
            LaterButton.IsVisible = false;
            ActionGrid.ColumnDefinitions.Clear();
            ActionGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            Grid.SetColumn(UpdateButton, 0);
            MandatoryNotice.IsVisible = true;

            TitleLabel.Text = "Update Required";
            MessageLabel.Text = "A critical update is required. Please update now to continue using the app.";
        }
        else
        {
            TitleLabel.Text = "Update Available";
            MessageLabel.Text = $"A newer version ({newVersion}) is available. Update now for the best experience.";
        }

        // Prevent closing popup by tapping outside when mandatory
        CanBeDismissedByTappingOutsideOfPopup = !isMandatory;
    }

    private async void OnUpdateClicked(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_updateUrl))
        {
            try
            {
                await Launcher.Default.OpenAsync(new Uri(_updateUrl));
            }
            catch
            {
                // URL could not be opened
            }
        }

        if (!_isMandatory)
        {
            // Close popup after opening store — optional update
            await CloseAsync();
        }
        // Mandatory: keep popup open so user must update — they'll come back from store
    }

    private async void OnLaterClicked(object? sender, EventArgs e)
    {
        await CloseAsync();
    }
}
