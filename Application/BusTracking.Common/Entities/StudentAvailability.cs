using BusTracking.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTracking.Common.Entities
{
    public class StudentAvailability
    {
        [Key] public int AvailabilityId { get; set; }
        public int StudentId { get; set; }
        public AvailabilityType AvailabilityType { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        [MaxLength(500)] public string? Remarks { get; set; }
        public int MarkedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(StudentId))] public StudentDetail Student { get; set; } = null!;
    }
}
