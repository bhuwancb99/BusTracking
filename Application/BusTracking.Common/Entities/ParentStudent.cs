namespace BusTracking.Common.Entities
{
    public class ParentStudent
    {
        [Key] public int ParentStudentId { get; set; }
        public int ParentId { get; set; }
        public int StudentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ParentId))] public ParentDetail Parent { get; set; } = null!;
        [ForeignKey(nameof(StudentId))] public StudentDetail Student { get; set; } = null!;
    }
}
