namespace BusTracking.Common.DTOs.Logger
{
    public class LogEntryDto
    {
        public string Platform { get; set; } = "";
        public string? ExceptionMessage { get; set; }
        public string? StackTrace { get; set; }
        public string? RequestUrl { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public string? ModuleName { get; set; }
        public string? ActionName { get; set; }
        public string? AdditionalDetails { get; set; }
    }
}
