namespace BusTracking.Mobile.Models.Subject
{
    public class CreateSubjectRequest
    {
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
    }
}
