namespace BusTracking.Common.DTOs.ClassMapping
{
    public class AssignClassSubjectTeacherDto
    {
        [Required(ErrorMessage = "Academic Session is required."), Range(1, int.MaxValue, ErrorMessage = "Please select an Academic Session.")]
        public int AcademicYearId { get; set; }

        [Required(ErrorMessage = "Class / Standard is required."), Range(1, int.MaxValue, ErrorMessage = "Please select a Class / Standard.")]
        public int StandardId { get; set; }

        [Required(ErrorMessage = "Section is required."), Range(1, int.MaxValue, ErrorMessage = "Please select a Section.")]
        public int SectionId { get; set; }

        [Required(ErrorMessage = "Subject is required."), Range(1, int.MaxValue, ErrorMessage = "Please select a Subject.")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Teacher is required."), Range(1, int.MaxValue, ErrorMessage = "Please select a Teacher.")]
        public int TeacherId { get; set; }
    }
}
