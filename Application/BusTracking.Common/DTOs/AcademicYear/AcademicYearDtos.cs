namespace BusTracking.Common.DTOs.AcademicYear
{
    public class AcademicYearDto
    {
        public int AcademicYearId { get; set; }
        public int SchoolId { get; set; }
        public string YearName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

