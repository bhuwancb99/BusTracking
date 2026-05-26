using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.Bus;
using BusTracking.Mobile.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminBusListViewModel : BaseViewModel
    {
        private readonly IBusService _buses;

        [ObservableProperty] private ObservableCollection<BusItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _canLoadMore;

        // Permissions
        public bool CanAdd => Can("bus.add");
        public bool CanEdit => Can("bus.edit");
        public bool CanDelete => Can("bus.delete");

        public AdminBusListViewModel(IAuthService auth, INavigationService nav, IBusService buses)
            : base(auth, nav) { _buses = buses; Title = "Buses"; }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _buses.GetAllAsync(SearchText.Trim().Length > 0 ? SearchText : null, 1);
                Items = new ObservableCollection<BusItem>(data);
                IsEmpty = !Items.Any();
                CanLoadMore = data.Count == 20;
            });
        }

        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            if (!CanLoadMore || IsBusy) return;
            await RunAsync(async () =>
            {
                CurrentPage++;
                var data = await _buses.GetAllAsync(SearchText, CurrentPage);
                foreach (var item in data) Items.Add(item);
                CanLoadMore = data.Count == 20;
            });
        }

        [RelayCommand]
        private async Task SearchAsync() => await LoadAsync();

        [RelayCommand]
        private Task AddAsync() => Nav.GoToAsync("AdminBusForm");

        [RelayCommand]
        private Task EditAsync(BusItem bus) =>
            Nav.GoToAsync("AdminBusForm", new Dictionary<string, object> { ["BusId"] = bus.BusId });

        [RelayCommand]
        private Task ViewDetailsAsync(BusItem bus) =>
            Nav.GoToAsync("AdminBusDetail", new Dictionary<string, object> { ["BusId"] = bus.BusId });

        [RelayCommand]
        private async Task ToggleAsync(BusItem bus)
        {
            var r = await _buses.ToggleAsync(bus.BusId);
            if (r.Success) { await ShowToastAsync(r.Message); await LoadAsync(); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync(BusItem bus)
        {
            if (!await ConfirmAsync("Delete Bus", $"Delete '{bus.BusName}'?")) return;
            var r = await _buses.DeleteAsync(bus.BusId);
            if (r.Success) { Items.Remove(bus); await ShowToastAsync("Bus deleted."); }
            else SetError(r.Message);
        }
    }
}
