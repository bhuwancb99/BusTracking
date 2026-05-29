namespace BusTracking.Mobile.Views;

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
            // First visit — full init
            _initialized = true;
            await ViewModel.InitializeAsync();
        }
        else
        {
            // Returning from a child page (e.g. after Add/Edit form) — refresh list
            await ViewModel.RefreshOnReturnAsync();
        }
    }
}