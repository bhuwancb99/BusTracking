namespace BusTracking.Mobile.Models.Coordinator
{
    public partial class PermissionItem : ObservableObject
    {
        public int Id { get; set; }
        public string ModuleName { get; set; } = "";
        public string Key { get; set; } = "";
        public string Description { get; set; } = "";
        [ObservableProperty] private bool _isSelected;
    }
}
