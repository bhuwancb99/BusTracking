namespace BusTracking.Mobile.Viewmodels.SuperAdmin
{
    public partial class AdminFeedbackDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IApiService _api;

        [ObservableProperty] private int _feedbackId;
        [ObservableProperty] private FeedbackItem? _feedback;
        [ObservableProperty] private string _selectedStatus = "";

        public bool CanManage => true; // Super Admin always can manage status
        public List<string> StatusOptions => ["Open", "InProgress", "Resolved", "Closed"];

        public AdminFeedbackDetailViewModel(IAuthService auth, INavigationService nav, IApiService api)
            : base(auth, nav) { _api = api; Title = "Feedback Detail"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("FeedbackId", out var id)) FeedbackId = (int)id;
        }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                // Re-use coordinator endpoint to fetch single feedback by id since it's shared and authorized
                var r = await _api.GetAsync<FeedbackItem>(
                    string.Format(Constants.Coordinator.FeedbackById, FeedbackId));
                Feedback = r.Data;
                SelectedStatus = Feedback?.Status ?? "Open";
            });
        }

        [RelayCommand]
        private async Task UpdateStatusAsync()
        {
            await RunAsync(async () =>
            {
                // Update using SuperAdmin specific status endpoint
                var r = await _api.PutAsync<object>(
                    string.Format(Constants.Admin.FeedbackStatus, FeedbackId),
                    new { Status = SelectedStatus });
                if (r.Success)
                {
                    await ShowToastAsync("Status updated.");
                    await LoadAsync();
                }
                else SetError(r.Message);
            });
        }

        [RelayCommand] private Task BackAsync() => Nav.GoBackAsync();
    }
}
