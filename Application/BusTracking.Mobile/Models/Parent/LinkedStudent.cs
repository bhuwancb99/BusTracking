namespace BusTracking.Mobile.Models.Parent
{
    public class LinkedStudent
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Standard { get; set; }
        public string? BusNumber { get; set; }
        public string? BusName { get; set; }

        public string BusDisplay => BusName != null ? $"{BusName} ({BusNumber})" : "No bus";
    }
}
