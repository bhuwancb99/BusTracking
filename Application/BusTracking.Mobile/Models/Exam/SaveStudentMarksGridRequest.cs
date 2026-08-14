namespace BusTracking.Mobile.Models.Exam
{
    public class SaveStudentMarksGridRequest
    {
        public int ExamScheduleId { get; set; }
        public List<SaveStudentMarksItemRequest> Marks { get; set; } = [];
    }
}
