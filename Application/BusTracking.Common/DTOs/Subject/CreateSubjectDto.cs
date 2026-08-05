namespace BusTracking.Common.DTOs.Subject
{
    public class CreateSubjectDto
    {
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
    }
}
