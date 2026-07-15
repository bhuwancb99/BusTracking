namespace BusTracking.Common.Entities
{
    public class SubAdminPermission : IMultiTenant
    {
        public int? SchoolId { get; set; }

        [Key] public int SubAdminPermissionId { get; set; }
        public int UserId { get; set; }
        public int PermissionId { get; set; }
        public int AssignedBy { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))] public User User { get; set; } = null!;
        [ForeignKey(nameof(PermissionId))] public Permission Permission { get; set; } = null!;
    }
}
