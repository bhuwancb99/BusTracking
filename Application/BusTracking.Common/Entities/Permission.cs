namespace BusTracking.Common.Entities
{
    public class Permission
    {
        [Key] public int PermissionId { get; set; }
        [Required, MaxLength(100)] public string ModuleName { get; set; } = "";
        [Required, MaxLength(100)] public string PermissionKey { get; set; } = "";
        [MaxLength(255)] public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<SubAdminPermission> SubAdminPermissions { get; set; } = [];
    }
}
