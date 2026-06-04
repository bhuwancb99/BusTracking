namespace BusTracking.Mobile.Models.Permission
{
    public partial class SelectablePermission : ObservableObject
    {
        public int Id { get; set; }
        public string Key { get; set; } = "";
        public string Label { get; set; } = "";
        public string ModuleName { get; set; } = "";
        [ObservableProperty] private bool _isSelected;
    }
}
