namespace BusTracking.Mobile.Models.Exam
{
    public class ExamScheduleItem
    {
        public int ExamScheduleId { get; set; }
        public int ExamTermId { get; set; }
        public string TermName { get; set; } = "";
        public int StandardId { get; set; }
        public string StandardName { get; set; } = "";
        public int? SectionId { get; set; }
        public string SectionName { get; set; } = "";
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = "";
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal PassMarks { get; set; }
        public string? RoomNumber { get; set; }

        public string ExamDateDisplay => ExamDate.ToString("ddd, dd MMM yyyy");
        public string TimeDisplay => $"{DateTime.Today.Add(StartTime):hh:mm tt} - {DateTime.Today.Add(EndTime):hh:mm tt}";
        public string MarksDisplay => $"Max: {MaxMarks:0.#} | Pass: {PassMarks:0.#}";
        public string RoomDisplay => string.IsNullOrWhiteSpace(RoomNumber) ? "Room: TBA" : $"Room: {RoomNumber}";
        public string ClassDisplay => string.IsNullOrWhiteSpace(SectionName) ? StandardName : $"{StandardName} ({SectionName})";
    }
}
