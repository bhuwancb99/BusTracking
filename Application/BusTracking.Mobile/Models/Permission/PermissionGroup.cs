namespace BusTracking.Mobile.Models.Permission
{
    public partial class PermissionGroup : ObservableObject
    {
        [ObservableProperty] private string _moduleName = "";
        [ObservableProperty] private ObservableCollection<PermissionItem> _permissions = [];
        public bool AreAllSelected => Permissions.All(p => p.IsSelected);
    }
}
