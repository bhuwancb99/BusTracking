namespace BusTracking.Mobile.Views;

public abstract class ViewBase<TViewModel> : ContentPage
    where TViewModel : BaseViewModel
{
    protected TViewModel ViewModel { get; }
    private bool _initialized;
    private Grid? _overlayGrid;

    protected ViewBase(TViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        SetupLoaderOverlay();
    }

    private void SetupLoaderOverlay()
    {
        if (BindingContext is BaseViewModel && Content != null && _overlayGrid == null)
        {
            var originalContent = Content;
            if (originalContent is Grid grid && grid.StyleId == "RootLoaderWrapGrid")
            {
                return;
            }

            var rootGrid = new Grid
            {
                StyleId = "RootLoaderWrapGrid",
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            Content = null;
            rootGrid.Children.Add(originalContent);

            _overlayGrid = new Grid
            {
                BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#66000000"), // soft semi-transparent black overlay
                ZIndex = 9999,
                InputTransparent = false,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            _overlayGrid.SetBinding(VisualElement.IsVisibleProperty, new Binding("IsBusy"));

            // Card container (dark rounded rectangle in the center)
            var cardBorder = new Border
            {
                BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#2d2d2d"),
                Stroke = Microsoft.Maui.Graphics.Colors.Transparent,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = new CornerRadius(16)
                },
                Padding = new Thickness(24, 24),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 220,
                HeightRequest = 150
            };

            var stack = new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Spacing = 16
            };

            var spinner = new ActivityIndicator
            {
                Color = Microsoft.Maui.Graphics.Colors.White, // white spinner color as requested
                HeightRequest = 48, // increased loader spinner size
                WidthRequest = 48,
                HorizontalOptions = LayoutOptions.Center
            };
            spinner.SetBinding(ActivityIndicator.IsRunningProperty, new Binding("IsBusy"));

            var label = new Label
            {
                Text = "Please Wait....",
                TextColor = Microsoft.Maui.Graphics.Colors.White,
                FontAttributes = FontAttributes.Bold,
                FontSize = 15, // increased size of text
                HorizontalOptions = LayoutOptions.Center
            };

            stack.Children.Add(spinner);
            stack.Children.Add(label);

            cardBorder.Content = stack;
            _overlayGrid.Children.Add(cardBorder);
            rootGrid.Children.Add(_overlayGrid);

            Content = rootGrid;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        SetupLoaderOverlay();

        if (!_initialized)
        {
            // First visit - full init
            _initialized = true;
            await ViewModel.InitializeAsync();
        }
        else
        {
            // Returning from a child page (e.g. after Add/Edit form) - refresh list
            await ViewModel.RefreshOnReturnAsync();
        }
    }
}