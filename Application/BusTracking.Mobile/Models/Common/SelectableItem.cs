namespace BusTracking.Mobile.Models.Common
{
    public partial class SelectableItem : ObservableObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Code { get; set; }

        [ObservableProperty]
        private bool _isSelected;

        public string Display => !string.IsNullOrEmpty(Code) ? $"{Name} ({Code})" : Name;
    }
}
