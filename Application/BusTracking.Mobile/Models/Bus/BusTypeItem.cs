namespace BusTracking.Mobile.Models.Bus
{
    public class BusTypeItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int BusCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string BusCountLabel => BusCount == 1 ? "1 bus" : $"{BusCount} buses";
        public string UpdatedLabel  => UpdatedAt.ToLocalTime().ToString("dd MMM yyyy");
    }
}
