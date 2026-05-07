using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTracking.Common.Entities
{
    public class User
    {
        [Key] public int UserId { get; set; }
        public int RoleId { get; set; }
        [Required, MaxLength(150)] public string FullName { get; set; } = "";
        [Required, MaxLength(255)] public string Email { get; set; } = "";
        [MaxLength(20)] public string? PhoneNumber { get; set; }
        [Required, MaxLength(512)] public string PasswordHash { get; set; } = "";
        [Required, MaxLength(256)] public string PasswordSalt { get; set; } = "";
        [MaxLength(500)] public string? ProfileImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsEmailVerified { get; set; } = false;
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; } = null!;

        public StudentDetail? StudentDetail { get; set; }
        public ParentDetail? ParentDetail { get; set; }
        public DriverDetail? DriverDetail { get; set; }
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
        public ICollection<SubAdminPermission> SubAdminPermissions { get; set; } = [];
        public ICollection<Notification> Notifications { get; set; } = [];
        public ICollection<DeviceToken> DeviceTokens { get; set; } = [];
        public ICollection<Feedback> Feedbacks { get; set; } = [];
    }
}
