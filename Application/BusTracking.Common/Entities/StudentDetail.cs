using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTracking.Common.Entities
{
    public class StudentDetail
    {
        [Key] public int StudentId { get; set; }
        public int UserId { get; set; }
        [Required, MaxLength(50)] public string StudentCode { get; set; } = "";
        [MaxLength(50)] public string? Standard { get; set; }
        public int? BusId { get; set; }
        public int? StopId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))] public User User { get; set; } = null!;
        [ForeignKey(nameof(BusId))] public Bus? Bus { get; set; }
        [ForeignKey(nameof(StopId))] public Stop? Stop { get; set; }

        public ICollection<ParentStudent> ParentStudents { get; set; } = [];
        public ICollection<StudentAvailability> Availabilities { get; set; } = [];
        public ICollection<StudentTripStatus> TripStatuses { get; set; } = [];
    }
}
