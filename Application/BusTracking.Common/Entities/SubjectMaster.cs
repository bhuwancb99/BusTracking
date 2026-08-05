namespace BusTracking.Common.Entities
{
    [Table("Subjects")]
    public class SubjectMaster : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key]
        public int SubjectId { get; set; }

        [Required, MaxLength(150)]
        public string SubjectName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SubjectCode { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(SchoolId))]
        public virtual School? School { get; set; }
    }
}
