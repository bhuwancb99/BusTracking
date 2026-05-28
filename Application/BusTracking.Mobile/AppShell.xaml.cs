namespace BusTracking.Mobile;

public partial class AppShell : Shell
{
    /// <summary>
    /// BackPressCounter
    /// </summary>
    int BackPressCounter = 0;

    public AppShell()
    {
        InitializeComponent();
    }

#pragma warning disable CS8602
    /// <summary>
    /// OnBackButtonPressed
    /// </summary>
    /// <returns></returns>
    protected override bool OnBackButtonPressed()
    {
        try
        {
            if (BackPressCounter == 2)
            {
#if ANDROID
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#endif
#if IOS
                        //add your ios platform code to close application
#endif
            }
            else if (Navigation.NavigationStack.Count == 1)
            {
                BackPressCounter++;
#if ANDROID
                Android.Widget.Toast.MakeText(Android.App.Application.Context, "Double tap to exit", Android.Widget.ToastLength.Long).Show();
#endif
#if IOS
                       //add your ios platform code to close application
#endif
            }
            else
            {
                Navigation.PopAsync();
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return true;
    }
#pragma warning restore CS8602
}