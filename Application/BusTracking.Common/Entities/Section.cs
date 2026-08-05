namespace BusTracking.Common.Entities
{
    [Table("Sections")]
    public class Section : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key]
        public int SectionId { get; set; }

        public int StandardId { get; set; }

        [Required, MaxLength(50)]
        public string SectionName { get; set; } = "A";

        public bool IsDefault { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? ClassTeacherId { get; set; }

        [ForeignKey(nameof(StandardId))]
        public virtual StandardMaster Standard { get; set; } = null!;

        [ForeignKey(nameof(ClassTeacherId))]
        public virtual Teacher? ClassTeacher { get; set; }

        [ForeignKey(nameof(SchoolId))]
        public virtual School? School { get; set; }
    }
}
