namespace BusTracking.Mobile.Models.Subject
{
    public class UpdateSubjectRequest
    {
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
        public bool IsActive { get; set; }
    }
}
