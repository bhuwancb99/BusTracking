namespace BusTracking.Common.DTOs.AcademicYear
{
    public class CreateAcademicYearRequest
    {
        public int SchoolId { get; set; }
        public string YearName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
        public bool SetAsCurrent { get; set; } = false;
    }
}
