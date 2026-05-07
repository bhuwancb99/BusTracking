using BusTracking.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTracking.Common.Entities
{
    public class DeviceToken
    {
        [Key] public int DeviceTokenId { get; set; }
        public int UserId { get; set; }
        [Required, MaxLength(512)] public string Token { get; set; } = "";
        public DevicePlatform Platform { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))] public User User { get; set; } = null!;
    }
}
