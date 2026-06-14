namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordBusTypeListViewModel : BaseViewModel
    {
        private readonly IBusTypeService _busTypes;

        [ObservableProperty] private ObservableCollection<BusTypeItem> _items = [];
        [ObservableProperty] private string _searchText = "";

        [ObservableProperty] private string _formName = "";
        [ObservableProperty] private bool _isFormOpen;
        [ObservableProperty] private bool _isEditing;
        private int _editingId;

        public string SearchPlaceholder => "Search bus types…";

        // Granular permission properties — match web: bustype.view/add/edit/delete
        public bool CanView => Can("bustype.view");
        public bool CanAdd => Can("bustype.add");
        public bool CanEdit => Can("bustype.edit");
        public bool CanDelete => Can("bustype.delete");

        public CoordBusTypeListViewModel(IAuthService auth, INavigationService nav,
            IBusTypeService busTypes) : base(auth, nav)
        {
            Title = "Bus Types";
            _busTypes = busTypes;
        }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _busTypes.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null);
                Items = new ObservableCollection<BusTypeItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand]
        private async Task SearchAsync() => await LoadAsync();

        [RelayCommand]
        private void OpenAdd()
        {
            _editingId = 0;
            FormName = "";
            IsEditing = false;
            IsFormOpen = true;
        }

        [RelayCommand]
        private void OpenEdit(BusTypeItem item)
        {
            _editingId = item.Id;
            FormName = item.Name;
            IsEditing = true;
            IsFormOpen = true;
        }

        [RelayCommand]
        private void CloseForm() { IsFormOpen = false; FormName = ""; }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(FormName))
            {
                SetError("Bus type name is required.");
                return;
            }
            await RunAsync(async () =>
            {
                var r = IsEditing
                    ? await _busTypes.UpdateAsync(_editingId, FormName.Trim())
                    : await _busTypes.CreateAsync(FormName.Trim());

                if (r.Success)
                {
                    IsFormOpen = false;
                    FormName = "";
                    await ShowToastAsync(r.Message ?? (IsEditing ? "Updated." : "Created."));
                    await LoadAsync();
                }
                else SetError(r.Message);
            });
        }

        [RelayCommand]
        private async Task DeleteAsync(BusTypeItem item)
        {
            if (!await ConfirmAsync("Delete Bus Type",
                    $"Delete '{item.Name}'? This cannot be undone.")) return;

            await RunAsync(async () =>
            {
                var r = await _busTypes.DeleteAsync(item.Id);
                if (r.Success) { await ShowToastAsync(r.Message ?? "Deleted."); await LoadAsync(); }
                else SetError(r.Message);
            });
        }
    }
}
