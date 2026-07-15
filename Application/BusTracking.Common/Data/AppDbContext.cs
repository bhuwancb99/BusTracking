namespace BusTracking.Common.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<School> Schools { get; set; }
    public DbSet<SystemAdministrator> SystemAdministrators { get; set; }

    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<SubAdminPermission> SubAdminPermissions { get; set; }
    public DbSet<BusRoute> Routes { get; set; }
    public DbSet<Stop> Stops { get; set; }
    public DbSet<Bus> Buses { get; set; }
    public DbSet<BusImage> BusImages { get; set; }
    public DbSet<BusTypeMaster> BusTypeMasters { get; set; }
    public DbSet<DriverDetail> DriverDetails { get; set; }
    public DbSet<StudentDetail> Students { get; set; }
    public DbSet<StandardMaster> StandardMasters { get; set; }
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
    public DbSet<AppConfiguration> AppConfigurations { get; set; }
    public DbSet<Logger> Loggers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<School>().ToTable("Schools");
        modelBuilder.Entity<SystemAdministrator>().ToTable("SystemAdministrators");
        // ── Explicit table names — must match SQL script exactly ─────
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<Permission>().ToTable("Permissions");
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<PasswordResetToken>().ToTable("PasswordResetTokens");
        modelBuilder.Entity<SubAdminPermission>().ToTable("SubAdminPermissions");
        modelBuilder.Entity<BusRoute>().ToTable("Routes");
        modelBuilder.Entity<Stop>().ToTable("Stops");
        modelBuilder.Entity<Bus>().ToTable("Buses");
        modelBuilder.Entity<BusImage>().ToTable("BusImages");
        modelBuilder.Entity<BusTypeMaster>().ToTable("BusTypeMasters");
        modelBuilder.Entity<DriverDetail>().ToTable("DriverDetails");
        modelBuilder.Entity<StudentDetail>().ToTable("Students");
        modelBuilder.Entity<StandardMaster>().ToTable("StandardMasters");
        modelBuilder.Entity<ParentDetail>().ToTable("Parents");
        modelBuilder.Entity<ParentStudent>().ToTable("ParentStudents");
        modelBuilder.Entity<StudentAvailability>().ToTable("StudentAvailabilities");
        modelBuilder.Entity<BusTrip>().ToTable("BusTrips");
        modelBuilder.Entity<TripStopEvent>().ToTable("TripStopEvents");
        modelBuilder.Entity<StudentTripStatus>().ToTable("StudentTripStatus");
        modelBuilder.Entity<BusLiveLocation>().ToTable("BusLiveLocation");
        modelBuilder.Entity<Notification>().ToTable("Notifications");
        modelBuilder.Entity<NotificationSetting>().ToTable("NotificationSettings");
        modelBuilder.Entity<DeviceToken>().ToTable("DeviceTokens");
        modelBuilder.Entity<Feedback>().ToTable("Feedbacks");
        modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");
        modelBuilder.Entity<AppConfiguration>().ToTable("AppConfigurations");
        modelBuilder.Entity<Logger>().ToTable("Logger");

        // ── Unique indexes ────────────────────────────────────────────
        modelBuilder.Entity<School>().HasIndex(s => s.SchoolCode).IsUnique();
        modelBuilder.Entity<SystemAdministrator>().HasIndex(s => s.UserName).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<BusRoute>().HasIndex(r => r.RouteCode).IsUnique();
        modelBuilder.Entity<Bus>().HasIndex(b => b.BusNumber).IsUnique();
        modelBuilder.Entity<StudentDetail>().HasIndex(s => s.StudentCode).IsUnique();
        modelBuilder.Entity<StandardMaster>().HasIndex(s => s.StandardName).IsUnique();
        modelBuilder.Entity<Permission>().HasIndex(p => p.PermissionKey).IsUnique();
        modelBuilder.Entity<PasswordResetToken>().HasIndex(t => t.Token).IsUnique();
        modelBuilder.Entity<BusTypeMaster>().HasIndex(b => b.Name).IsUnique();
        modelBuilder.Entity<SubAdminPermission>()
            .HasIndex(sp => new { sp.UserId, sp.PermissionId }).IsUnique();
        modelBuilder.Entity<ParentStudent>()
            .HasIndex(ps => new { ps.ParentId, ps.StudentId }).IsUnique();
        modelBuilder.Entity<Stop>()
            .HasIndex(s => new { s.RouteId, s.StopOrder }).IsUnique();
        modelBuilder.Entity<StudentTripStatus>()
            .HasIndex(sts => new { sts.TripId, sts.StudentId }).IsUnique();

        // ── Enum → string storage ─────────────────────────────────────
        modelBuilder.Entity<BusTrip>().Property(t => t.TripType).HasConversion<string>();
        modelBuilder.Entity<BusTrip>().Property(t => t.Status).HasConversion<string>();
        modelBuilder.Entity<StudentTripStatus>().Property(s => s.BoardingStatus).HasConversion<string>();
        modelBuilder.Entity<StudentAvailability>().Property(a => a.AvailabilityType).HasConversion<string>();
        modelBuilder.Entity<Feedback>().Property(f => f.Category).HasConversion<string>();
        modelBuilder.Entity<Feedback>().Property(f => f.Status).HasConversion<string>();
        modelBuilder.Entity<Notification>().Property(n => n.NotificationType).HasConversion<string>();
        modelBuilder.Entity<DeviceToken>().Property(d => d.Platform).HasConversion<string>();
        modelBuilder.Entity<TripStopEvent>().Property(t => t.Status).HasConversion<string>();
        modelBuilder.Entity<AppConfiguration>().Property(c => c.Platform).HasConversion<string>();
        modelBuilder.Entity<AppConfiguration>().HasIndex(c => new { c.ConfigKey, c.Platform }).IsUnique();

        // ── Prevent cascade cycles ────────────────────────────────────
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

        // BusImage → Bus (cascade delete)
        modelBuilder.Entity<BusImage>()
            .HasOne(i => i.Bus).WithMany(b => b.Images)
            .HasForeignKey(i => i.BusId).OnDelete(DeleteBehavior.Cascade);

        // Bus → BusTypeMaster (restrict delete — can't delete a type in use)
        modelBuilder.Entity<Bus>()
            .HasOne(b => b.BusType).WithMany(t => t.Buses)
            .HasForeignKey(b => b.BusTypeId).OnDelete(DeleteBehavior.Restrict);

        // StudentDetail → StandardMaster (restrict delete)
        modelBuilder.Entity<StudentDetail>()
            .HasOne(s => s.Standard).WithMany()
            .HasForeignKey(s => s.StandardId).OnDelete(DeleteBehavior.Restrict);

        // ── Performance indexes ───────────────────────────────────────
        modelBuilder.Entity<BusLiveLocation>()
            .HasIndex(l => new { l.TripId, l.RecordedAt });
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.RecipientUserId, n.IsRead, n.SentAt });
        modelBuilder.Entity<StudentAvailability>()
            .HasIndex(a => new { a.StudentId, a.FromDate, a.ToDate });
        modelBuilder.Entity<Feedback>()
            .HasIndex(f => new { f.Status, f.CreatedAt });
        modelBuilder.Entity<BusImage>()
            .HasIndex(i => i.BusId);

        // ── Multi-Tenant Global Query Filters ─────────────────────────
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var schoolIdProperty = Expression.Property(parameter, nameof(IMultiTenant.SchoolId));
                
                var dbContextExpression = Expression.Constant(this);
                var currentSchoolIdProperty = Expression.Property(dbContextExpression, nameof(CurrentSchoolId));
                
                var equalsExpression = Expression.Equal(schoolIdProperty, currentSchoolIdProperty);
                var isNullExpression = Expression.Equal(currentSchoolIdProperty, Expression.Constant(null, typeof(int?)));
                var combinedExpression = Expression.OrElse(equalsExpression, isNullExpression);
                
                var lambda = Expression.Lambda(combinedExpression, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public int? CurrentSchoolId => _currentUserService.SchoolId;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var schoolId = _currentUserService.SchoolId;
        if (schoolId.HasValue)
        {
            foreach (var entry in ChangeTracker.Entries<IMultiTenant>())
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity.SchoolId == null || entry.Entity.SchoolId == 0)
                    {
                        entry.Entity.SchoolId = schoolId.Value;
                    }
                }
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}