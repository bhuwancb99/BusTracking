namespace BusTracking.Mobile.Interfaces
{
    /// <summary>
    /// Navigation service — wraps Shell navigation for DI-friendly use.
    /// </summary>
    public interface INavigationService
    {
        Task GoToAsync(string route, bool animate = true);
        Task GoToAsync(string route, Dictionary<string, object> parameters, bool animate = true);
        Task GoBackAsync();
        Task GoToLoginAsync();
        Task GoToDashboardAsync(string role);
    }
}
