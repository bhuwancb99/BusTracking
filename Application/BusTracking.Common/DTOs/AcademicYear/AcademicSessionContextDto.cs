namespace BusTracking.Common.DTOs.AcademicYear
{
    public class AcademicSessionContextDto
    {
        public int ActiveAcademicYearId { get; set; }
        public string ActiveYearName { get; set; } = string.Empty;
        public List<AcademicYearDto> AvailableYears { get; set; } = new();
    }
}
