namespace BusTracking.Common.Entities
{
    public class ParentDetail : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int ParentId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))] public User User { get; set; } = null!;
        public ICollection<ParentStudent> ParentStudents { get; set; } = [];
    }
}
