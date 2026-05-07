using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTracking.Common.Entities
{
    public class DriverDetail
    {
        [Key] public int DriverDetailId { get; set; }
        public int UserId { get; set; }
        [MaxLength(100)] public string? LicenseNumber { get; set; }
        public DateOnly? LicenseExpiry { get; set; }
        public int? BusId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))] public User User { get; set; } = null!;
        [ForeignKey(nameof(BusId))] public Bus? Bus { get; set; }
    }
}
