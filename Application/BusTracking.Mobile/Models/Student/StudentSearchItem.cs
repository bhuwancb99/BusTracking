namespace BusTracking.Mobile.Models.Student
{
    public class StudentSearchItem
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public int? StandardId { get; set; }
        public string? StandardName { get; set; }
        public string? BusNumber { get; set; }

        /// <summary>
        /// Shown in search results dropdown: "Name (Code - Class)"
        /// </summary>
        public string DisplayLabel =>
            StandardName is { Length: > 0 }
                ? $"{FullName} ({StudentCode} - {StandardName})"
                : $"{FullName} ({StudentCode})";
    }
}
