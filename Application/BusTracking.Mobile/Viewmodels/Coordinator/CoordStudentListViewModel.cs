namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordStudentListViewModel : BaseViewModel
    {
        private readonly IStudentService _students;

        [ObservableProperty] private ObservableCollection<StudentItem> _items = [];
        [ObservableProperty] private string _searchText = "";
        [ObservableProperty] private string _selectedFilter = "Active";   // Active | Inactive | Both
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private bool _canLoadMore;

        public string SearchPlaceholder => "Search students…";
        public List<string> FilterOptions => ["Active", "Inactive", "Both"];
        public bool CanAdd => Can("student.add");
        public bool CanEdit => Can("student.edit");
        public bool CanDelete => Can("student.delete");

        public CoordStudentListViewModel(IAuthService auth, INavigationService nav, IStudentService students)
            : base(auth, nav) { _students = students; Title = "Students"; }

        public override async Task InitializeAsync() => await LoadAsync();
        public override async Task RefreshOnReturnAsync() => await LoadAsync();

        // Re-load when filter chip changes
        partial void OnSelectedFilterChanged(string value) => LoadCommand.ExecuteAsync(null);

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                CurrentPage = 1;
                var data = await _students.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedFilter);
                Items = new ObservableCollection<StudentItem>(data.Items);
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
                var data = await _students.GetAllAsync(
                    SearchText.Trim().Length > 0 ? SearchText.Trim() : null, CurrentPage, SelectedFilter);
                foreach (var item in data.Items) Items.Add(item);
                CanLoadMore = data.PageNumber < data.TotalPages;
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("CoordStudentForm");
        [RelayCommand]
        private Task EditAsync(StudentItem s) =>
            Nav.GoToAsync("CoordStudentForm", new Dictionary<string, object> { ["StudentId"] = s.StudentId });
        [RelayCommand]
        private Task DetailAsync(StudentItem s) =>
            Nav.GoToAsync("CoordStudentDetail", new Dictionary<string, object> { ["StudentId"] = s.StudentId });

        [RelayCommand]
        private void Filter(string filter) => SelectedFilter = filter;
        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            try
            {
                await LoadAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }
}
