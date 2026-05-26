using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.Parent;
using BusTracking.Mobile.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminParentListViewModel : BaseViewModel
    {
        private readonly IParentService _parents;

        [ObservableProperty] private ObservableCollection<ParentItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private bool _canLoadMore;
        private int _page = 1;

        public bool CanAdd => Can("parent.add");
        public bool CanEdit => Can("parent.edit");
        public bool CanDelete => Can("parent.delete");

        public AdminParentListViewModel(IAuthService auth, INavigationService nav, IParentService parents)
            : base(auth, nav) { _parents = parents; Title = "Parents"; }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                _page = 1;
                var data = await _parents.GetAllAsync(SearchText.Trim().Length > 0 ? SearchText : null);
                Items = new ObservableCollection<ParentItem>(data);
                IsEmpty = !Items.Any();
                CanLoadMore = data.Count == 20;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("AdminParentForm");
        [RelayCommand]
        private Task EditAsync(ParentItem p) =>
            Nav.GoToAsync("AdminParentForm", new Dictionary<string, object> { ["UserId"] = p.UserId });
        [RelayCommand]
        private Task ViewAsync(ParentItem p) =>
            Nav.GoToAsync("AdminParentDetail", new Dictionary<string, object> { ["UserId"] = p.UserId });

        [RelayCommand]
        private async Task ToggleAsync(ParentItem p)
        {
            var r = await _parents.ToggleAsync(p.UserId);
            if (r.Success) await LoadAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task ResetPasswordAsync(ParentItem p)
        {
            if (!await ConfirmAsync("Reset Password", $"Reset password for {p.FullName}?")) return;
            var r = await _parents.ResetPasswordAsync(p.UserId);
            if (r.Success) await ShowAlertAsync("Password Reset", r.Message);
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync(ParentItem p)
        {
            if (!await ConfirmAsync("Delete Parent", $"Delete '{p.FullName}'?")) return;
            var r = await _parents.DeleteAsync(p.UserId);
            if (r.Success) { Items.Remove(p); await ShowToastAsync("Parent deleted."); }
            else SetError(r.Message);
        }
    }
}
