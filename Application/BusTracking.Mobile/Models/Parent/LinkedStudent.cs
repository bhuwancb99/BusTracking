namespace BusTracking.Mobile.Models.Parent
{
    public class LinkedStudent
    {
        private string? _busNumber;
        private string? _busName;
        private int? _stopId;
        private string? _stopName;

        public int StudentId { get; set; }
        public string StudentCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public int? StandardId { get; set; }
        public string? StandardName { get; set; }

        public LinkedStudentBusInfo? Bus { get; set; }
        public LinkedStudentStopInfo? Stop { get; set; }

        public string? BusNumber
        {
            get => _busNumber ?? Bus?.BusNumber;
            set => _busNumber = value;
        }

        public string? BusName
        {
            get => _busName ?? Bus?.BusName;
            set => _busName = value;
        }

        public int? StopId
        {
            get => _stopId ?? Stop?.StopId;
            set => _stopId = value;
        }

        public string? StopName
        {
            get => _stopName ?? Stop?.StopName;
            set => _stopName = value;
        }

        public string BusDisplay
        {
            get
            {
                var name = BusName;
                var num = BusNumber;
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(num))
                    return name.Equals(num, System.StringComparison.OrdinalIgnoreCase) ? name : $"{name} ({num})";
                if (!string.IsNullOrEmpty(name)) return name;
                if (!string.IsNullOrEmpty(num)) return num;
                return "No bus";
            }
        }

        /// <summary>Initials from first two words, e.g. "Rahul Pandey" → "RP"</summary>
        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FullName)) return "?";
                var parts = FullName.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                return string.Concat(parts.Take(2).Select(w => char.ToUpper(w[0])));
            }
        }
    }
}
