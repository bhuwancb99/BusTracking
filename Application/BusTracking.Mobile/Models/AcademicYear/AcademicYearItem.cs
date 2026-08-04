namespace BusTracking.Mobile.Models.AcademicYear
{
    public class AcademicYearItem
    {
        public int AcademicYearId { get; set; }
        public int SchoolId { get; set; }
        public string YearName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsCurrent { get; set; }
    }
}
