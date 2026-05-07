using System.ComponentModel.DataAnnotations;

namespace BusTracking.Common.Entities
{
    public class Role
    {
        [Key] public int RoleId { get; set; }
        [Required, MaxLength(50)] public string RoleName { get; set; } = "";
        [MaxLength(255)] public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<User> Users { get; set; } = [];
    }
}
