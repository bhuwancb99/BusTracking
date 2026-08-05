namespace BusTracking.Common.DTOs.Attendance
{
    public class FaceAttendanceScanResultDto
    {
        public int TotalDetectedFaces { get; set; }
        public int MatchedStudentsCount { get; set; }
        public List<StudentAttendanceRowDto> RecognizedStudents { get; set; } = new();
        public List<StudentAttendanceRowDto> AllClassStudents { get; set; } = new();
    }
}
