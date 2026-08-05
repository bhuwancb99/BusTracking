namespace BusTracking.Common.DTOs.Subject
{
    public class UpdateSubjectDto
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
        public bool IsActive { get; set; }
    }
}
