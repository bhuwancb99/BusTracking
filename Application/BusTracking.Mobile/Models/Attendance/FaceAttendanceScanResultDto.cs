namespace BusTracking.Mobile.Models.Attendance
{
    public class FaceAttendanceScanResultDto
    {
        public int TotalDetectedFaces { get; set; }
        public int TotalFacesDetected { get => TotalDetectedFaces; set => TotalDetectedFaces = value; }
        public int MatchedStudentsCount { get; set; }
        public int MatchedCount { get => MatchedStudentsCount; set => MatchedStudentsCount = value; }
        public int UnmatchedCount => Math.Max(0, TotalFacesDetected - MatchedCount);
        public List<StudentAttendanceRowDto> RecognizedStudents { get; set; } = new();
        public List<StudentAttendanceRowDto> AllClassStudents { get; set; } = new();
    }
}
