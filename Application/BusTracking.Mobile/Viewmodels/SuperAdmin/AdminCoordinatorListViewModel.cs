using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.Coordinator;
using BusTracking.Mobile.Services;
using BusTracking.Mobile.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminCoordinatorListViewModel : BaseViewModel
    {
        private readonly ICoordinatorService _coords;

        [ObservableProperty] private ObservableCollection<CoordinatorItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private bool _canLoadMore;

        public AdminCoordinatorListViewModel(IAuthService auth, INavigationService nav, ICoordinatorService coords)
            : base(auth, nav) { _coords = coords; Title = "Bus Coordinators"; }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _coords.GetAllAsync(SearchText.Trim().Length > 0 ? SearchText : null);
                Items = new ObservableCollection<CoordinatorItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminCoordinatorForm");
        [RelayCommand]
        private Task EditAsync(CoordinatorItem c) =>
            Nav.GoToAsync("AdminCoordinatorForm", new Dictionary<string, object> { ["UserId"] = c.UserId });
        [RelayCommand]
        private Task ViewAsync(CoordinatorItem c) =>
            Nav.GoToAsync("AdminCoordinatorDetail", new Dictionary<string, object> { ["UserId"] = c.UserId });

        [RelayCommand]
        private async Task ToggleAsync(CoordinatorItem c)
        {
            var r = await _coords.ToggleAsync(c.UserId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task ResetPasswordAsync(CoordinatorItem c)
        {
            if (!await ConfirmAsync("Reset Password", $"Reset password for {c.FullName}?")) return;
            var r = await _coords.ResetPasswordAsync(c.UserId);
            if (r.Success) await ShowAlertAsync("Password Reset", r.Message);
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync(CoordinatorItem c)
        {
            if (!await ConfirmAsync("Delete", $"Delete coordinator '{c.FullName}'?")) return;
            var r = await _coords.DeleteAsync(c.UserId);
            if (r.Success) { Items.Remove(c); await ShowToastAsync("Coordinator deleted."); }
            else SetError(r.Message);
        }
    }
}
