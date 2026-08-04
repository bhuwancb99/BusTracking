namespace BusTracking.Common.DTOs.AcademicYear
{
    public class UpdateAcademicYearRequest
    {
        public int AcademicYearId { get; set; }
        public string YearName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }
        public bool SetAsCurrent { get; set; }
    }
}
