using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.Driver;
using BusTracking.Mobile.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordDriverListViewModel : BaseViewModel
    {
        private readonly IDriverService _drivers;

        [ObservableProperty] private ObservableCollection<DriverItem> _items = [];
        [ObservableProperty] private string _searchText = "";

        public bool CanAdd => Can("driver.add");
        public bool CanEdit => Can("driver.edit");
        public bool CanView => Can("driver.view");

        public CoordDriverListViewModel(IAuthService auth, INavigationService nav, IDriverService drivers)
            : base(auth, nav) { _drivers = drivers; Title = "Drivers"; }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _drivers.GetAllAsync(SearchText.Trim().Length > 0 ? SearchText : null);
                Items = new ObservableCollection<DriverItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand]
        private Task ViewAsync(DriverItem d) =>
            Nav.GoToAsync("CoordDriverDetail", new Dictionary<string, object> { ["UserId"] = d.UserId });
    }
}
