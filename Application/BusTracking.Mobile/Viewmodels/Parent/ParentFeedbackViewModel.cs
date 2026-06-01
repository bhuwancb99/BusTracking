namespace BusTracking.Mobile.Viewmodels.Parent
{
    public partial class ParentFeedbackViewModel : BaseViewModel
    {
        private readonly IApiService _api;

        [ObservableProperty]
        private List<string> _categories =
        [
            "Bus Delay",
            "Route Issue",
            "Driver Behavior",
            "App Problem",
            "Missing Child Record",
            "Other"
        ];

        [ObservableProperty] private string? _selectedCategory;
        [ObservableProperty] private string _subject = "";
        [ObservableProperty] private string _message = "";

        public ParentFeedbackViewModel(IAuthService auth, INavigationService nav, IApiService api)
            : base(auth, nav) { _api = api; Title = "Help & Support"; }

        [RelayCommand]
        private async Task SubmitAsync()
        {
            if (string.IsNullOrWhiteSpace(Subject))
            { SetError("Please enter a subject."); return; }
            if (string.IsNullOrWhiteSpace(Message))
            { SetError("Please enter a message."); return; }

            await RunAsync(async () =>
            {
                var req = new
                {
                    Category = SelectedCategory ?? "Other",
                    Subject = Subject.Trim(),
                    Message = Message.Trim()
                };

                var r = await _api.PostAsync<object>(Constants.Common.Feedback, req);
                if (r.Success)
                {
                    Subject = "";
                    Message = "";
                    SelectedCategory = null;
                    await ShowToastAsync("Your message has been submitted. We'll respond soon.");
                }
                else
                {
                    SetError(r.Message ?? "Failed to submit feedback.");
                }
            });
        }
    }
}
