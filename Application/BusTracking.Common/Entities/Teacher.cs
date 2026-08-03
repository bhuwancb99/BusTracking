namespace BusTracking.Common.Entities
{
    [Table("Teachers")]
    public class Teacher : IMultiTenant
    {
        [Key]
        public int TeacherId { get; set; }

        public int UserId { get; set; }

        public int? SchoolId { get; set; }

        [MaxLength(50)]
        public string? EmployeeCode { get; set; }

        [MaxLength(150)]
        public string? Qualification { get; set; }

        [MaxLength(100)]
        public string? Designation { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        public DateTime? JoiningDate { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(20)]
        public string? EmergencyContact { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(SchoolId))]
        public virtual School? School { get; set; }
    }
}
