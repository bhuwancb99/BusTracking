namespace BusTracking.Mobile.Models.Parent
{
    public class LinkedStudent
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public int? StandardId { get; set; }
        public string? StandardName { get; set; }
        public string? BusNumber { get; set; }
        public string? BusName { get; set; }

        public string BusDisplay => BusName != null ? $"{BusName} ({BusNumber})" : "No bus";

        /// <summary>Initials from first two words, e.g. "Rahul Pandey" → "RP"</summary>
        public string Initials
        {
            get
            {
                var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return string.Concat(parts.Take(2).Select(w => char.ToUpper(w[0])));
            }
        }
    }
}
