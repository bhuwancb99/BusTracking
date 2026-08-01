using BusTracking.Mobile.Models.Feedback;

namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordFeedbackDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IApiService _api;

        [ObservableProperty] private int _feedbackId;
        [ObservableProperty] private FeedbackItem? _feedback;
        [ObservableProperty] private string _selectedStatus = "";

        public bool CanManage => Can("helpsupport.manage");
        public List<string> StatusOptions => ["Open", "InProgress", "Resolved", "Closed"];

        public CoordFeedbackDetailViewModel(IAuthService auth, INavigationService nav, IApiService api)
            : base(auth, nav) { _api = api; Title = "Feedback Detail"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Feedback", out var fb) && fb is FeedbackItem item)
            {
                Feedback = item;
                FeedbackId = item.FeedbackId;
                SelectedStatus = item.Status ?? "Open";
            }
            else if (query.TryGetValue("FeedbackId", out var id))
            {
                FeedbackId = (int)id;
            }
        }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            if (FeedbackId <= 0 && Feedback is null) return;
            await RunAsync(async () =>
            {
                if (FeedbackId > 0)
                {
                    var r = await _api.GetAsync<FeedbackItem>(
                        string.Format(Constants.Coordinator.FeedbackById, FeedbackId));
                    if (r.Success && r.Data is not null)
                    {
                        Feedback = r.Data;
                        SelectedStatus = Feedback.Status ?? "Open";
                    }
                }
            });
        }

        [RelayCommand]
        private async Task UpdateStatusAsync()
        {
            if (!CanManage) return;
            await RunAsync(async () =>
            {
                var r = await _api.PutAsync<object>(
                    string.Format(Constants.Coordinator.FeedbackUpdateStatus, FeedbackId),
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
