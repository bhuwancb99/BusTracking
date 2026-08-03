namespace BusTracking.Common.Entities
{
    [Table("SubAdminPermissions")]
    public class SubAdminPermission : IMultiTenant
    {
        public int? SchoolId { get; set; }
        public int UserId { get; set; }
        public int PermissionId { get; set; }

        [Column("GrantedAt")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [Column("GrantedBy")]
        public int? AssignedBy { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(PermissionId))]
        public virtual Permission Permission { get; set; } = null!;

        [ForeignKey(nameof(AssignedBy))]
        public virtual User? AssignedByUser { get; set; }
    }
}
