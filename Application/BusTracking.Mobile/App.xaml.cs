namespace BusTracking.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = Handler?.MauiContext?.Services.GetRequiredService<AppShell>()
                        ?? IPlatformApplication.Current!.Services.GetRequiredService<AppShell>();
            return new Window(shell);
        }
    }
}
