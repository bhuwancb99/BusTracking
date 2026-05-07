namespace BusTracking.Common.Models
{
    public class BreadcrumbItem
    {
        public string Label { get; set; } = "";
        public string? Url { get; set; }          // null = current (no link)
        public bool IsActive => Url is null;
    }
}
