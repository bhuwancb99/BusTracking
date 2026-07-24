#pragma warning disable CA1416

namespace BusTracking.Mobile.Views.Student
{
    public partial class StudentTrackingPage : ViewBase<StudentTrackingViewModel>
    {
        private readonly IAppConfigService _appConfig;
        private bool _webViewReady;
        private readonly List<string> _pendingJs = new();
        private readonly object _pendingJsLock = new();

        public StudentTrackingPage(StudentTrackingViewModel vm, IAppConfigService appConfig) : base(vm)
        {
            InitializeComponent();
            _appConfig = appConfig;

            vm.SendToMap = js =>
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (!_webViewReady)
                    {
                        lock (_pendingJsLock) { _pendingJs.Add(js); }
                        return;
                    }
                    try { await MapWebView.EvaluateJavaScriptAsync(js); } catch { }
                });

            if (MapWebView != null)
            {
                MapWebView.Navigated += (s, e) =>
                {
                    _webViewReady = true;
                    List<string> copy;
                    lock (_pendingJsLock)
                    {
                        copy = new List<string>(_pendingJs);
                        _pendingJs.Clear();
                    }
                    foreach (var js in copy)
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            try { await MapWebView.EvaluateJavaScriptAsync(js); } catch { }
                        });
                    }
                };
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (MapWebView != null)
            {
                var html = await GoogleMapKeyHolder.GetMapHtmlAsync();
                MapWebView.Source = new HtmlWebViewSource { Html = html };
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            ViewModel.StopPolling();
        }
    }
}