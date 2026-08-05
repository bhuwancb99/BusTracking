namespace BusTracking.Common.Entities
{
    [Table("ClassSubjectTeachers")]
    public class ClassSubjectTeacher : IMultiTenant
    {
        [Key]
        public int ClassSubjectTeacherId { get; set; }

        public int? SchoolId { get; set; }

        public int AcademicYearId { get; set; }

        public int StandardId { get; set; }

        public int SectionId { get; set; }

        public int SubjectId { get; set; }

        public int TeacherId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(SchoolId))]
        public virtual School? School { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [ForeignKey(nameof(StandardId))]
        public virtual StandardMaster Standard { get; set; } = null!;

        [ForeignKey(nameof(SectionId))]
        public virtual Section Section { get; set; } = null!;

        [ForeignKey(nameof(SubjectId))]
        public virtual SubjectMaster Subject { get; set; } = null!;

        [ForeignKey(nameof(TeacherId))]
        public virtual Teacher Teacher { get; set; } = null!;
    }
}
