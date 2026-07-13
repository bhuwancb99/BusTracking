using CommunityToolkit.Maui.Extensions;

namespace BusTracking.Mobile.Views.Common;

public partial class ConfirmPopup : CommunityToolkit.Maui.Views.Popup
{
    public ConfirmPopup(string title, string message, string accept = "Yes", string cancel = "No", string iconSource = "logout.png", Color? acceptColor = null)
    {
        InitializeComponent();

        TitleLabel.Text = title;
        MessageLabel.Text = message;
        AcceptButton.Text = accept;
        CancelButton.Text = cancel;

        if (!string.IsNullOrEmpty(iconSource))
        {
            IconImage.Source = iconSource;
        }

        if (acceptColor != null)
        {
            AcceptButton.BackgroundColor = acceptColor;
            IconTintColor.TintColor = acceptColor;

            // Adjust the badge background color matching the accent using FromArgb to avoid obsolete warnings
            if (acceptColor.ToHex() == "#BA1A1A" || acceptColor.ToHex() == "#ba1a1a")
            {
                IconBadgeBorder.BackgroundColor = AppThemeBindingEvaluator(Color.FromArgb("#FFF0F0"), Color.FromArgb("#401A1A"));
            }
            else
            {
                IconBadgeBorder.BackgroundColor = AppThemeBindingEvaluator(Color.FromArgb("#F0F4FF"), Color.FromArgb("#1A2440"));
            }
        }
    }

    private Color AppThemeBindingEvaluator(Color light, Color dark)
    {
        return Application.Current?.RequestedTheme == AppTheme.Dark ? dark : light;
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        if (Application.Current?.Windows[0].Page is Page p)
        {
            await p.ClosePopupAsync(false);
        }
    }

    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (Application.Current?.Windows[0].Page is Page p)
        {
            await p.ClosePopupAsync(true);
        }
    }
}
