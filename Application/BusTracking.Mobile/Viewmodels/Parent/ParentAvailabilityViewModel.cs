namespace BusTracking.Mobile.Viewmodels.Parent
{
    public partial class ParentAvailabilityViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IParentService _parents;
        private readonly IStudentService _students;

        [ObservableProperty] private ObservableCollection<LinkedStudent> _children = [];
        [ObservableProperty] private LinkedStudent? _selectedChild;
        [ObservableProperty] private DateTime _selectedDate = DateTime.Today;
        [ObservableProperty] private bool _morningAvailable = true;
        [ObservableProperty] private bool _eveningAvailable = true;
        [ObservableProperty] private string _reason = "";

        private int _preselectedStudentId;

        public DateTime MinimumDate => DateTime.Today;

        public ParentAvailabilityViewModel(IAuthService auth, INavigationService nav,
            IParentService parents, IStudentService students)
            : base(auth, nav)
        {
            _parents = parents;
            _students = students;
            Title = "Availability";
        }

        /// <summary>
        /// Receives optional PreselectedStudentId when navigated from a child's Leave button.
        /// </summary>
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("PreselectedStudentId", out var id))
                _preselectedStudentId = (int)id;
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () =>
            {
                // Load linked children from parent dashboard API
                var raw = await _parents.GetDashboardAsync();
                var childList = new List<LinkedStudent>();

                if (raw is System.Text.Json.JsonElement je && je.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (je.TryGetProperty("children", out var el) ||
                        je.TryGetProperty("Students", out el) ||
                        je.TryGetProperty("students", out el))
                    {
                        childList = System.Text.Json.JsonSerializer.Deserialize<List<LinkedStudent>>(
                            el.GetRawText(),
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                    }
                }

                Children = new ObservableCollection<LinkedStudent>(childList);

                // Auto-select: if single child, select automatically;
                // if preselected from dashboard tap, select that child.
                if (_preselectedStudentId > 0)
                    SelectedChild = Children.FirstOrDefault(c => c.StudentId == _preselectedStudentId) ?? Children.FirstOrDefault();
                else if (Children.Count == 1)
                    SelectedChild = Children[0];
            });
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (SelectedChild is null)
            {
                SetError("Please select a child first.");
                return;
            }

            await RunAsync(async () =>
            {
                var req = new
                {
                    StudentId = SelectedChild.StudentId,
                    Date = SelectedDate,
                    MorningAvailable,
                    EveningAvailable,
                    Reason
                };

                // Post to the parent availability endpoint
                var r = await _students.SetAvailabilityAsync(req);
                if (r.Success)
                {
                    Reason = "";
                    MorningAvailable = true;
                    EveningAvailable = true;
                    await ShowToastAsync($"Availability saved for {SelectedChild.FullName}.");
                }
                else
                {
                    SetError(r.Message ?? "Failed to save availability.");
                }
            });
        }
    }
}
