namespace BusTracking.Mobile.Views.Common
{
    public partial class TripStartAlertPopupPage : ContentPage
    {
        private readonly IRingtoneService _ringtoneService;

        public TripStartAlertPopupPage(
            IRingtoneService ringtoneService,
            string driverName,
            string? driverPhotoUrl,
            string busName,
            string routeName,
            string fromStop,
            string toStop)
        {
            InitializeComponent();
            _ringtoneService = ringtoneService;

            LblDriverName.Text = string.IsNullOrWhiteSpace(driverName) ? "Bus Driver" : driverName;
            LblBusInfo.Text = string.IsNullOrWhiteSpace(busName) ? "School Bus" : busName;
            LblRouteName.Text = string.IsNullOrWhiteSpace(routeName) ? "Active Route" : routeName;
            LblFromStop.Text = string.IsNullOrWhiteSpace(fromStop) ? "Origin" : fromStop;
            LblToStop.Text = string.IsNullOrWhiteSpace(toStop) ? "Destination" : toStop;

            if (!string.IsNullOrWhiteSpace(driverPhotoUrl))
            {
                ImgDriverPhoto.Source = ImageSource.FromUri(new Uri(driverPhotoUrl));
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Start continuous call ringing sound
            _ringtoneService.PlayRingtone();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Ensure ringtone is stopped when page disappears
            _ringtoneService.StopRingtone();
        }

        private async void OnCloseButtonClicked(object sender, EventArgs e)
        {
            _ringtoneService.StopRingtone();
            await Navigation.PopModalAsync();
        }
    }
}
