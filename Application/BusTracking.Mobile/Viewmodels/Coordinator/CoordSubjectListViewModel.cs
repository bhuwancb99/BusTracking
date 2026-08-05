namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordSubjectListViewModel : BaseViewModel
    {
        private readonly ISubjectService _subjectService;

        [ObservableProperty] private ObservableCollection<SubjectItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _canLoadMore;

        public string SearchPlaceholder => "Search subjects…";
        public bool CanAdd => true;

        public CoordSubjectListViewModel(IAuthService auth, INavigationService nav, ISubjectService subjectService)
            : base(auth, nav)
        {
            _subjectService = subjectService;
            Title = "Subject Master";
        }

        public override async Task InitializeAsync() => await LoadSubjectsAsync();
        public override async Task RefreshOnReturnAsync() => await LoadSubjectsAsync();

        [RelayCommand]
        private async Task LoadSubjectsAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _subjectService.GetAllAsync(SearchText, CurrentPage, isCoordinator: true);
                Items = new ObservableCollection<SubjectItem>(data.Items);
                IsEmpty = !Items.Any();
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            if (!CanLoadMore || IsBusy) return;
            await RunAsync(async () =>
            {
                CurrentPage++;
                var data = await _subjectService.GetAllAsync(SearchText, CurrentPage, isCoordinator: true);
                foreach (var item in data.Items) Items.Add(item);
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadSubjectsAsync();
        }

        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("CoordSubjectForm");

        [RelayCommand]
        private Task EditAsync(SubjectItem s) =>
            Nav.GoToAsync("CoordSubjectForm", new Dictionary<string, object> { ["SubjectId"] = s.SubjectId });

        [RelayCommand]
        private async Task ToggleAsync(SubjectItem s)
        {
            var r = await _subjectService.ToggleAsync(s.SubjectId, isCoordinator: true);
            if (r.Success) await LoadSubjectsAsync(); else SetError(r.Message);
        }

        [RelayCommand]
        private async Task DeleteAsync(SubjectItem s)
        {
            if (!await ConfirmAsync("Delete Subject", $"Delete Subject '{s.SubjectName}'?")) return;
            var r = await _subjectService.DeleteAsync(s.SubjectId, isCoordinator: true);
            if (r.Success) { Items.Remove(s); await ShowToastAsync("Subject deleted."); }
            else SetError(r.Message);
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try { await LoadSubjectsAsync(); }
            finally { IsRefreshing = false; }
        }
    }
}
