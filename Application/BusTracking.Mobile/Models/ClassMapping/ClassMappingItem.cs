namespace BusTracking.Mobile.Models.ClassMapping
{
    public class ClassMappingItem
    {
        public int Id { get; set; }
        public int ClassSubjectTeacherId { get => Id; set => Id = value; }
        public int AcademicYearId { get; set; }
        public string AcademicYearName { get; set; } = string.Empty;
        public int StandardId { get; set; }
        public string StandardName { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;

        public string SummaryLabel => $"{StandardName} - Section {SectionName} | {SubjectName}";
    }
}
