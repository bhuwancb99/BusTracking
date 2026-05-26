namespace BusTracking.Mobile.Models.Coordinator
{
    public class PermissionItem
    {
        public int Id { get; set; }
        public string ModuleName { get; set; } = "";
        public string Key { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsSelected { get; set; }
    }
}
