namespace BusTracking.Mobile.Views.Common;

/// <summary>
/// Themed single-button alert popup — matches ConfirmPopup visual style.
/// Use instead of DisplayAlert for a consistent app-wide look.
/// </summary>
public partial class AlertPopup : CommunityToolkit.Maui.Views.Popup
{
    public AlertPopup(string title, string message, string okText = "OK",
        string iconSource = "info.png", Color? iconColor = null)
    {
        InitializeComponent();

        TitleLabel.Text = title;
        MessageLabel.Text = message;
        OkButton.Text = okText;

        if (!string.IsNullOrEmpty(iconSource))
        {
            IconImage.Source = iconSource;
        }

        if (iconColor != null)
        {
            IconTintColor.TintColor = iconColor;

            // Danger style (red)
            if (iconColor.ToHex().Contains("1A1A", StringComparison.OrdinalIgnoreCase) ||
                iconColor.ToHex().Contains("BA1A", StringComparison.OrdinalIgnoreCase))
            {
                IconBadgeBorder.BackgroundColor = AppThemeBindingEvaluator(
                    Color.FromArgb("#FFF0F0"), Color.FromArgb("#401A1A"));
            }
            // Warning style (amber/orange)
            else if (iconColor.ToHex().Contains("FFA9", StringComparison.OrdinalIgnoreCase) ||
                     iconColor.ToHex().Contains("F59E", StringComparison.OrdinalIgnoreCase))
            {
                IconBadgeBorder.BackgroundColor = AppThemeBindingEvaluator(
                    Color.FromArgb("#FFF8E1"), Color.FromArgb("#40351A"));
            }
            // Info / Primary style (blue)
            else
            {
                IconBadgeBorder.BackgroundColor = AppThemeBindingEvaluator(
                    Color.FromArgb("#F0F4FF"), Color.FromArgb("#1A2440"));
            }
        }
    }

    private Color AppThemeBindingEvaluator(Color light, Color dark)
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark ? dark : light;
    }

    private async void OnOkClicked(object? sender, EventArgs e)
    {
        await CloseAsync();
    }
}
