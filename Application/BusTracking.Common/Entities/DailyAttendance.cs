namespace BusTracking.Common.Entities
{
    [Table("DailyAttendances")]
    public class DailyAttendance : IMultiTenant
    {
        [Key]
        public long AttendanceId { get; set; }

        public int? SchoolId { get; set; }

        public int AcademicYearId { get; set; }

        public int StandardId { get; set; }

        public int? SectionId { get; set; }

        public int? SubjectId { get; set; }

        public int StudentId { get; set; }

        public DateTime AttendanceDate { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Present"; // Present, Absent, Late, Excused

        public bool IsFaceScanned { get; set; } = false;

        [MaxLength(500)]
        public string? PhotoUrl { get; set; }

        public int? MarkedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(SchoolId))]
        public virtual School? School { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [ForeignKey(nameof(StandardId))]
        public virtual StandardMaster Standard { get; set; } = null!;

        [ForeignKey(nameof(SectionId))]
        public virtual Section? Section { get; set; }

        [ForeignKey(nameof(SubjectId))]
        public virtual SubjectMaster? Subject { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual StudentDetail Student { get; set; } = null!;

        [ForeignKey(nameof(MarkedByUserId))]
        public virtual User? MarkedByUser { get; set; }
    }
}
