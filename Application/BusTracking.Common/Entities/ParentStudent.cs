namespace BusTracking.Common.Entities
{
    [Table("ParentStudents")]
    public class ParentStudent : IMultiTenant
    {
        public int? SchoolId { get; set; }
        public int ParentId { get; set; }
        public int StudentId { get; set; }

        [MaxLength(50)]
        public string? Relationship { get; set; } = "Parent";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ParentId))]
        public virtual ParentDetail Parent { get; set; } = null!;

        [ForeignKey(nameof(StudentId))]
        public virtual StudentDetail Student { get; set; } = null!;
    }
}
