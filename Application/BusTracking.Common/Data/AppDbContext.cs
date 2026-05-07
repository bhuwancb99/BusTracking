using BusTracking.Common.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusTracking.Common.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── DbSets ───────────────────────────────────────────────────────
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<SubAdminPermission> SubAdminPermissions { get; set; }
        public DbSet<BusRoute> Routes { get; set; }
        public DbSet<Stop> Stops { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<DriverDetail> DriverDetails { get; set; }
        public DbSet<StudentDetail> Students { get; set; }
        public DbSet<ParentDetail> Parents { get; set; }
        public DbSet<ParentStudent> ParentStudents { get; set; }
        public DbSet<StudentAvailability> StudentAvailabilities { get; set; }
        public DbSet<BusTrip> BusTrips { get; set; }
        public DbSet<TripStopEvent> TripStopEvents { get; set; }
        public DbSet<StudentTripStatus> StudentTripStatuses { get; set; }
        public DbSet<BusLiveLocation> BusLiveLocations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationSetting> NotificationSettings { get; set; }
        public DbSet<DeviceToken> DeviceTokens { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Table names (match manual SQL script) ────────────────────
            modelBuilder.Entity<BusRoute>().ToTable("Routes");
            modelBuilder.Entity<StudentDetail>().ToTable("Students");
            modelBuilder.Entity<ParentDetail>().ToTable("Parents");
            modelBuilder.Entity<DriverDetail>().ToTable("DriverDetails");

            // ── Unique constraints ───────────────────────────────────────
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<BusRoute>().HasIndex(r => r.RouteCode).IsUnique();
            modelBuilder.Entity<Bus>().HasIndex(b => b.BusNumber).IsUnique();
            modelBuilder.Entity<StudentDetail>().HasIndex(s => s.StudentCode).IsUnique();
            modelBuilder.Entity<Permission>().HasIndex(p => p.PermissionKey).IsUnique();
            modelBuilder.Entity<PasswordResetToken>().HasIndex(t => t.Token).IsUnique();

            modelBuilder.Entity<SubAdminPermission>()
                .HasIndex(sp => new { sp.UserId, sp.PermissionId }).IsUnique();

            modelBuilder.Entity<ParentStudent>()
                .HasIndex(ps => new { ps.ParentId, ps.StudentId }).IsUnique();

            modelBuilder.Entity<Stop>()
                .HasIndex(s => new { s.RouteId, s.StopOrder }).IsUnique();

            modelBuilder.Entity<StudentTripStatus>()
                .HasIndex(sts => new { sts.TripId, sts.StudentId }).IsUnique();

            // ── Enum → string conversions ────────────────────────────────
            modelBuilder.Entity<BusTrip>()
                .Property(t => t.TripType).HasConversion<string>();
            modelBuilder.Entity<BusTrip>()
                .Property(t => t.Status).HasConversion<string>();
            modelBuilder.Entity<StudentTripStatus>()
                .Property(s => s.BoardingStatus).HasConversion<string>();
            modelBuilder.Entity<StudentAvailability>()
                .Property(a => a.AvailabilityType).HasConversion<string>();
            modelBuilder.Entity<Feedback>()
                .Property(f => f.Category).HasConversion<string>();
            modelBuilder.Entity<Feedback>()
                .Property(f => f.Status).HasConversion<string>();
            modelBuilder.Entity<Notification>()
                .Property(n => n.NotificationType).HasConversion<string>();
            modelBuilder.Entity<DeviceToken>()
                .Property(d => d.Platform).HasConversion<string>();
            modelBuilder.Entity<TripStopEvent>()
                .Property(t => t.Status).HasConversion<string>();

            // ── Prevent cascade delete cycles ────────────────────────────
            modelBuilder.Entity<SubAdminPermission>()
                .HasOne(sp => sp.User).WithMany(u => u.SubAdminPermissions)
                .HasForeignKey(sp => sp.UserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BusTrip>()
                .HasOne(t => t.Driver).WithMany()
                .HasForeignKey(t => t.DriverId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentTripStatus>()
                .HasOne(s => s.Student).WithMany(sd => sd.TripStatuses)
                .HasForeignKey(s => s.StudentId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DriverDetail>()
                .HasOne(d => d.Bus).WithOne(b => b.Driver)
                .HasForeignKey<DriverDetail>(d => d.BusId).OnDelete(DeleteBehavior.SetNull);

            // ── Performance indexes ──────────────────────────────────────
            modelBuilder.Entity<BusLiveLocation>()
                .HasIndex(l => new { l.TripId, l.RecordedAt });

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.RecipientUserId, n.IsRead, n.SentAt });

            modelBuilder.Entity<StudentAvailability>()
                .HasIndex(a => new { a.StudentId, a.FromDate, a.ToDate });

            modelBuilder.Entity<Feedback>()
                .HasIndex(f => new { f.Status, f.CreatedAt });
        }
    }
}
