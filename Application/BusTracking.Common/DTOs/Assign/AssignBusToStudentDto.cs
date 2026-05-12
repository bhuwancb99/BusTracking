namespace BusTracking.Common.DTOs.Assign
{
    public class AssignBusToStudentDto
    {
        public int StudentId { get; set; }
        public int? BusId { get; set; }
        public int? StopId { get; set; }
    }
}
