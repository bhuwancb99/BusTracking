namespace BusTracking.Mobile.Services
{
    public class NavigationService : INavigationService
    {
        public Task GoToAsync(string route, bool animate = true)
            => Shell.Current.GoToAsync(route, animate);

        public Task GoToAsync(string route, Dictionary<string, object> parameters, bool animate = true)
            => Shell.Current.GoToAsync(route, animate, parameters);

        public Task GoBackAsync()
            => Shell.Current.GoToAsync("..", true);

        public Task GoToLoginAsync()
            => Shell.Current.GoToAsync("//Login", true);

        public async Task GoToDashboardAsync(string role)
        {
            var route = role switch
            {
                Constants.Roles.SuperAdmin => "//AdminDashboard",
                Constants.Roles.BusCoordinator => "//CoordinatorDashboard",
                Constants.Roles.Parent => "//ParentDashboard",
                Constants.Roles.Student => "//StudentDashboard",
                Constants.Roles.Driver => "//DriverDashboard",
                _ => "//Login"
            };
            await Shell.Current.GoToAsync(route, true);
        }
    }
}
