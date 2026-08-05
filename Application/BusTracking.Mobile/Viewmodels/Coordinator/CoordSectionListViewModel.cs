namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordSectionListViewModel : BaseViewModel
    {
        private readonly ISectionService _sectionService;
        private readonly ICoordStandardService _standardService;

        [ObservableProperty] private ObservableCollection<StandardItem> _standards = [];
        [ObservableProperty] private StandardItem? _selectedStandard;
        [ObservableProperty] private ObservableCollection<SectionItem> _items = [];

        public bool CanAdd => true;

        public CoordSectionListViewModel(IAuthService auth, INavigationService nav, ISectionService sectionService, ICoordStandardService standardService)
            : base(auth, nav)
        {
            _sectionService = sectionService;
            _standardService = standardService;
            Title = "Class Sections";
        }

        public override async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                var stds = await _standardService.GetAllAsync(null, 1);
                Standards = new ObservableCollection<StandardItem>(stds.Items);
                SelectedStandard = Standards.FirstOrDefault();
                if (SelectedStandard != null)
                {
                    await FetchSectionsAsync(SelectedStandard.StandardId);
                }
            }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        partial void OnSelectedStandardChanged(StandardItem? value)
        {
            if (value != null) _ = FetchSectionsWithLoaderAsync(value.StandardId);
        }

        private async Task FetchSectionsWithLoaderAsync(int standardId)
        {
            IsBusy = true;
            try { await FetchSectionsAsync(standardId); }
            catch (Exception ex) { SetError(ex.Message); }
            finally { IsBusy = false; }
        }

        private async Task FetchSectionsAsync(int standardId)
        {
            var data = await _sectionService.GetByStandardAsync(standardId, isCoordinator: true);
            Items = new ObservableCollection<SectionItem>(data);
            IsEmpty = !Items.Any();
        }

        [RelayCommand]
        private Task AddAsync()
        {
            if (SelectedStandard is null) return Task.CompletedTask;
            return Nav.GoToAsync("CoordSectionForm", new Dictionary<string, object>
            {
                ["StandardId"] = SelectedStandard.StandardId,
                ["StandardName"] = SelectedStandard.StandardName
            });
        }

        [RelayCommand]
        private Task EditAsync(SectionItem s)
        {
            if (s is null || SelectedStandard is null) return Task.CompletedTask;
            return Nav.GoToAsync("CoordSectionForm", new Dictionary<string, object>
            {
                ["SectionId"] = s.SectionId,
                ["StandardId"] = SelectedStandard.StandardId,
                ["StandardName"] = SelectedStandard.StandardName
            });
        }

        [RelayCommand]
        private async Task ToggleStatusAsync(SectionItem s)
        {
            if (s is null) return;
            var r = await _sectionService.ToggleActiveAsync(s.SectionId, isCoordinator: true);
            if (r.Success)
            {
                s.IsActive = !s.IsActive;
                await ShowToastAsync($"Section status updated.");
                if (SelectedStandard != null) await FetchSectionsAsync(SelectedStandard.StandardId);
            }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync(SectionItem s)
        {
            if (!await ConfirmAsync("Delete Section", $"Delete Section '{s.SectionName}'?")) return;
            var r = await _sectionService.DeleteAsync(s.SectionId, isCoordinator: true);
            if (r.Success) { Items.Remove(s); IsEmpty = !Items.Any(); await ShowToastAsync("Section deleted."); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (SelectedStandard is null) return;
            IsRefreshing = true;
            try { await FetchSectionsAsync(SelectedStandard.StandardId); }
            finally { IsRefreshing = false; }
        }
    }
}
