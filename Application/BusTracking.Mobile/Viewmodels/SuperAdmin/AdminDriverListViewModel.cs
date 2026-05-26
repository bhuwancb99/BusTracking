using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.Driver;
using BusTracking.Mobile.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminDriverListViewModel : BaseViewModel
    {
        private readonly IDriverService _drivers;

        [ObservableProperty] private ObservableCollection<DriverItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private bool _canLoadMore;
        private int _page = 1;

        public bool CanAdd => Can("driver.add");
        public bool CanEdit => Can("driver.edit");
        public bool CanDelete => Can("driver.delete");

        public AdminDriverListViewModel(IAuthService auth, INavigationService nav, IDriverService drivers)
            : base(auth, nav) { _drivers = drivers; Title = "Drivers"; }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                _page = 1;
                var data = await _drivers.GetAllAsync(SearchText.Trim().Length > 0 ? SearchText : null, 1);
                Items = new ObservableCollection<DriverItem>(data);
                IsEmpty = !Items.Any();
                CanLoadMore = data.Count == 20;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();

        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminDriverForm");
        [RelayCommand]
        private Task EditAsync(DriverItem d) =>
            Nav.GoToAsync("AdminDriverForm", new Dictionary<string, object> { ["UserId"] = d.UserId });
        [RelayCommand]
        private Task ViewAsync(DriverItem d) =>
            Nav.GoToAsync("AdminDriverDetail", new Dictionary<string, object> { ["UserId"] = d.UserId });

        [RelayCommand]
        private async Task ToggleAsync(DriverItem d)
        {
            var r = await _drivers.ToggleAsync(d.UserId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task ResetPasswordAsync(DriverItem d)
        {
            if (!await ConfirmAsync("Reset Password", $"Reset password for {d.FullName}?")) return;
            var r = await _drivers.ResetPasswordAsync(d.UserId);
            if (r.Success) await ShowAlertAsync("Password Reset", $"New password has been generated.\n{r.Message}");
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync(DriverItem d)
        {
            if (!await ConfirmAsync("Delete Driver", $"Delete '{d.FullName}'?")) return;
            var r = await _drivers.DeleteAsync(d.UserId);
            if (r.Success) { Items.Remove(d); await ShowToastAsync("Driver deleted."); }
            else SetError(r.Message);
        }
    }
}
