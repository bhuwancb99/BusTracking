namespace BusTracking.Mobile.Views;

/// <summary>
/// Base page that automatically:
/// 1. Resolves ViewModel from DI
/// 2. Sets BindingContext
/// 3. Calls VM.InitializeAsync on first appear
/// 4. Passes Shell query parameters to ViewModel
///
/// All page code-behind files inherit this — no logic in .xaml.cs files.
/// </summary>
public abstract class ViewBase<TViewModel> : ContentPage
    where TViewModel : BaseViewModel
{
    protected TViewModel ViewModel { get; }
    private bool _initialized;

    protected ViewBase(TViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_initialized)
        {
            _initialized = true;
            await ViewModel.InitializeAsync();
        }
    }
}