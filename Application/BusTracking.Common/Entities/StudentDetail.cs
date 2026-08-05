namespace BusTracking.Common.Entities
{
    public class StudentDetail : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int StudentId { get; set; }
        public int UserId { get; set; }
        [Required, MaxLength(50)] public string StudentCode { get; set; } = "";
        public int? AcademicYearId { get; set; }
        public int? StandardId { get; set; }
        public int? SectionId { get; set; }
        public int? BusId { get; set; }
        public int? StopId { get; set; }

        // Transport Fee Tracking
        public TransportFeeStatus TransportFeeStatus { get; set; } = TransportFeeStatus.Paid;
        public DateOnly? FeeExpiryDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))] public User User { get; set; } = null!;
        [ForeignKey(nameof(AcademicYearId))] public AcademicYear? AcademicYear { get; set; }
        [ForeignKey(nameof(StandardId))] public StandardMaster? Standard { get; set; }
        [ForeignKey(nameof(SectionId))] public Section? Section { get; set; }
        [ForeignKey(nameof(BusId))] public Bus? Bus { get; set; }
        [ForeignKey(nameof(StopId))] public Stop? Stop { get; set; }

        public ICollection<ParentStudent> ParentStudents { get; set; } = [];
        public ICollection<StudentAvailability> Availabilities { get; set; } = [];
        public ICollection<StudentTripStatus> TripStatuses { get; set; } = [];
    }
}
