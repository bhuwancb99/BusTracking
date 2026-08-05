namespace BusTracking.Common.DTOs.ClassMapping
{
    public class ClassSubjectTeacherDto
    {
        public int ClassSubjectTeacherId { get; set; }
        public int AcademicYearId { get; set; }
        public string YearName { get; set; } = string.Empty;
        public int StandardId { get; set; }
        public string StandardName { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
