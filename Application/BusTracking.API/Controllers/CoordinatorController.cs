namespace BusTracking.API.Controllers
{
    /// <summary>
    /// API for Bus Coordinator MAUI app.
    /// All endpoints require role = BusCoordinator.
    /// </summary>
    [Authorize(Roles = "BusCoordinator"), Route("api/coordinator")]
    public class CoordinatorController : ApiBaseController
    {
        private readonly AppDbContext _db;
        private readonly IDashboardService _dash;
        private readonly ITripService _trip;
        private readonly ISubAdminService _subAdmin;
        private readonly IAppConfigService _appConfig;
        private readonly IFeedbackService _feedback;
        private readonly INotificationService _notif;
        private readonly IImageService _img;
        private readonly IRouteService _route;

        public CoordinatorController(AppDbContext db, IDashboardService dash, ITripService trip,
            ISubAdminService subAdmin, IAppConfigService appConfig, IFeedbackService feedback,
            INotificationService notif, IImageService img, IRouteService route)  // ← NEW
        {
            _db = db; _dash = dash; _trip = trip;
            _subAdmin = subAdmin; _appConfig = appConfig;
            _feedback = feedback; _notif = notif; _img = img; _route = route;
        }

        // ════════════════════════════════════════════════════════════
        // DASHBOARD
        // ════════════════════════════════════════════════════════════

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var r = await _dash.GetSummaryAsync();
            return Ok(r);
        }

        // ── POST api/coordinator/photo ────────────────────────────────
        /// <summary>
        /// Coordinator uploads or replaces their own profile photo.
        /// Send as multipart/form-data, field name "file".
        /// Returns: { success, data: "imageUrl", message }
        /// </summary>
        [HttpPost("photo")]
        [RequestSizeLimit(5_242_880)]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            var user = await _db.Users.FindAsync(CurrentUserId);
            if (user is null)
                return NotFound(ApiResponse<string>.Fail("User not found."));

            try
            {
                var url = await _img.SaveProfileImageAsync(
                    file, CurrentUserId, "coordinator", user.ProfileImageUrl);

                user.ProfileImageUrl = url;
                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<string>.Ok(url, "Profile photo updated."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        // ── DELETE api/coordinator/photo ──────────────────────────────
        [HttpDelete("photo")]
        public async Task<IActionResult> DeletePhoto()
        {
            var user = await _db.Users.FindAsync(CurrentUserId);
            if (user is null)
                return NotFound(ApiResponse<bool>.Fail("User not found."));

            _img.DeleteFile(user.ProfileImageUrl);
            user.ProfileImageUrl = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok(true, "Profile photo removed."));
        }

        // ════════════════════════════════════════════════════════════
        // TRIPS
        // ════════════════════════════════════════════════════════════

        [HttpGet("trips")]
        public async Task<IActionResult> Trips([FromQuery] string? status, [FromQuery] string? date)
        {
            var q = _db.BusTrips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TripStatus>(status, true, out var s))
                q = q.Where(t => t.Status == s);

            if (DateOnly.TryParse(date, out var d))
                q = q.Where(t => t.TripDate == d);
            else
                q = q.Where(t => t.TripDate == DateOnly.FromDateTime(DateTime.UtcNow));

            var trips = await q.OrderByDescending(t => t.TripDate).Take(50)
                .Select(t => new
                {
                    t.TripId,
                    t.TripDate,
                    TripType = t.TripType.ToString(),
                    Status = t.Status.ToString(),
                    BusNumber = t.Bus.BusNumber,
                    BusName = t.Bus.BusName,
                    DriverName = t.Driver != null ? t.Driver.FullName : null,
                    RouteName = t.Route != null ? t.Route.RouteName : null,
                    t.StartedAt,
                    t.EndedAt
                }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(trips));
        }

        [HttpGet("trips/{tripId}")]
        public async Task<IActionResult> TripDetail(int tripId)
        {
            var r = await _trip.GetByIdAsync(tripId);
            return r.Success ? Ok(r) : NotFound(r);
        }

        [HttpPost("trips")]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripRequest req)
        {
            if (!Enum.TryParse<TripType>(req.TripType, true, out _))
                return BadRequest(ApiResponse<bool>.Fail("Invalid trip type. Use Morning or Evening."));

            var dto = new CreateTripDto
            {
                BusId = req.BusId,
                RouteId = req.RouteId,
                TripType = req.TripType,
                TripDate = (req.TripDate ?? DateTime.UtcNow).ToString("yyyy-MM-dd")
            };

            var r = await _trip.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("trips/{tripId}/start")]
        public async Task<IActionResult> StartTrip(int tripId)
        {
            var trip = await _db.BusTrips.FindAsync(tripId);
            if (trip is null) return NotFound(ApiResponse<bool>.Fail("Trip not found."));
            trip.Status = TripStatus.InProgress;
            trip.StartedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Trip started."));
        }

        [HttpPost("trips/{tripId}/end")]
        public async Task<IActionResult> EndTrip(int tripId)
        {
            var trip = await _db.BusTrips.FindAsync(tripId);
            if (trip is null) return NotFound(ApiResponse<bool>.Fail("Trip not found."));
            trip.Status = TripStatus.Completed;
            trip.EndedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Trip completed."));
        }

        [HttpPost("trips/{tripId}/cancel")]
        public async Task<IActionResult> CancelTrip(int tripId)
        {
            var trip = await _db.BusTrips.FindAsync(tripId);
            if (trip is null) return NotFound(ApiResponse<bool>.Fail("Trip not found."));
            trip.Status = TripStatus.Cancelled;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Trip cancelled."));
        }

        [HttpGet("trips/{tripId}/location")]
        public async Task<IActionResult> TripLocation(int tripId)
        {
            var loc = await _db.BusLiveLocations
                .Where(l => l.TripId == tripId)
                .OrderByDescending(l => l.RecordedAt)
                .Select(l => new { l.Latitude, l.Longitude, l.Speed, l.Heading, l.RecordedAt })
                .FirstOrDefaultAsync();

            if (loc is null) return NotFound(ApiResponse<object>.Fail("No location data yet."));
            return Ok(ApiResponse<object>.Ok(loc));
        }

        [HttpGet("trips/{tripId}/location/history")]
        public async Task<IActionResult> LocationHistory(int tripId)
        {
            var history = await _db.BusLiveLocations
                .Where(l => l.TripId == tripId)
                .OrderBy(l => l.RecordedAt)
                .Select(l => new { l.Latitude, l.Longitude, l.Speed, l.Heading, l.RecordedAt })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(history));
        }

        // ════════════════════════════════════════════════════════════
        // BUSES
        // ════════════════════════════════════════════════════════════

        [HttpGet("buses")]
        public async Task<IActionResult> Buses([FromQuery] string? search)
        {
            var q = _db.Buses
                .Include(b => b.Route)
                .Include(b => b.Driver).ThenInclude(d => d!.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(b => b.BusName.Contains(search) || b.BusNumber.Contains(search));

            var buses = await q.Where(b => b.IsActive).OrderBy(b => b.BusName)
                .Select(b => new
                {
                    b.BusId,
                    b.BusName,
                    b.BusNumber,
                    b.Capacity,
                    RouteName = b.Route != null ? b.Route.RouteName : null,
                    DriverName = b.Driver != null ? b.Driver.User.FullName : null,
                    b.IsActive
                }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(buses));
        }

        [HttpGet("buses/{busId}")]
        public async Task<IActionResult> BusDetail(int busId)
        {
            var b = await _db.Buses
                .Include(x => x.Route).ThenInclude(r => r!.Stops)
                .Include(x => x.Driver).ThenInclude(d => d!.User)
                .Include(x => x.Students).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(x => x.BusId == busId);

            if (b is null) return NotFound(ApiResponse<object>.Fail("Bus not found."));

            return Ok(ApiResponse<object>.Ok(new
            {
                b.BusId,
                b.BusName,
                b.BusNumber,
                b.Capacity,
                b.IsActive,
                Route = b.Route is null ? null : new { b.Route.RouteId, b.Route.RouteName },
                Driver = b.Driver is null ? null : new { b.Driver.UserId, b.Driver.User.FullName, b.Driver.User.PhoneNumber },
                StudentCount = b.Students.Count
            }));
        }

        // ════════════════════════════════════════════════════════════
        // ROUTES
        // ════════════════════════════════════════════════════════════

        [HttpGet("routes")]
        public async Task<IActionResult> Routes([FromQuery] int page = 1, [FromQuery] string? search = null, [FromQuery] string? status = "Active")
        {
            var r = await _route.GetAllAsync(page, search, status);
            return Ok(r);
        }

        [HttpGet("routes/{routeId}/stops")]
        public async Task<IActionResult> RouteStops(int routeId)
        {
            var stops = await _db.Stops
                .Where(s => s.RouteId == routeId && s.IsActive)
                .OrderBy(s => s.StopOrder)
                .Select(s => new { s.StopId, s.StopName, s.StopOrder, s.Latitude, s.Longitude })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(stops));
        }

        // ════════════════════════════════════════════════════════════
        // DRIVERS
        // ════════════════════════════════════════════════════════════

        [HttpGet("drivers")]
        public async Task<IActionResult> Drivers([FromQuery] string? search)
        {
            var roleId = await _db.Roles.Where(r => r.RoleName == "Driver").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users
                .Include(u => u.DriverDetail).ThenInclude(d => d!.Bus)
                .Where(u => u.RoleId == roleId && u.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            var drivers = await q.OrderBy(u => u.FullName).Select(u => new
            {
                u.UserId,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.ProfileImageUrl,                             // ← includes photo
                BusNumber = u.DriverDetail != null && u.DriverDetail.Bus != null ? u.DriverDetail.Bus.BusNumber : null,
                BusName = u.DriverDetail != null && u.DriverDetail.Bus != null ? u.DriverDetail.Bus.BusName : null,
                LicenseNumber = u.DriverDetail != null ? u.DriverDetail.LicenseNumber : null,
                u.IsActive
            }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(drivers));
        }

        // ════════════════════════════════════════════════════════════
        // PARENTS
        // ════════════════════════════════════════════════════════════

        [HttpGet("parents")]
        public async Task<IActionResult> Parents([FromQuery] string? search, [FromQuery] int page = 1)
        {
            var roleId = await _db.Roles.Where(r => r.RoleName == "Parent").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users
                .Include(u => u.ParentDetail)
                    .ThenInclude(p => p!.ParentStudents).ThenInclude(ps => ps.Student)
                .Where(u => u.RoleId == roleId);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            var parents = await q.OrderBy(u => u.FullName).Skip((page - 1) * 20).Take(20)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    u.IsActive,
                    u.ProfileImageUrl,                         // ← includes photo
                    ChildrenCount = u.ParentDetail != null ? u.ParentDetail.ParentStudents.Count : 0
                }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(parents));
        }

        // ════════════════════════════════════════════════════════════
        // STUDENTS
        // ════════════════════════════════════════════════════════════

        [HttpGet("students")]
        public async Task<IActionResult> Students([FromQuery] string? search, [FromQuery] int page = 1)
        {
            var q = _db.Students
                .Include(s => s.User)
                .Include(s => s.Bus)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(s => s.User.FullName.Contains(search) || s.StudentCode.Contains(search));

            var students = await q.Where(s => s.User.IsActive)
                .OrderBy(s => s.User.FullName).Skip((page - 1) * 20).Take(20)
                .Select(s => new
                {
                    s.StudentId,
                    s.StudentCode,
                    FullName = s.User.FullName,
                    ProfileImageUrl = s.User.ProfileImageUrl,  // ← NEW
                    s.Standard,
                    BusNumber = s.Bus != null ? s.Bus.BusNumber : null,
                    BusName = s.Bus != null ? s.Bus.BusName : null,
                    s.User.IsActive
                }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(students));
        }

        [HttpGet("students/{id:int}")]
        public async Task<IActionResult> StudentById(int id)
        {
            var s = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Bus).ThenInclude(b => b!.Route)
                .Include(s => s.ParentStudents).ThenInclude(ps => ps.Parent).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (s is null) return NotFound(ApiResponse<object>.Fail("Student not found."));

            return Ok(ApiResponse<object>.Ok(new
            {
                s.StudentId,
                s.StudentCode,
                FullName = s.User.FullName,
                s.User.Email,
                s.User.PhoneNumber,
                ProfileImageUrl = s.User.ProfileImageUrl,      // ← NEW
                s.Standard,
                s.User.IsActive,
                Bus = s.Bus is null ? null : new { s.Bus.BusId, s.Bus.BusName, s.Bus.BusNumber },
                Route = s.Bus?.Route is null ? null : new { s.Bus.Route.RouteId, s.Bus.Route.RouteName },
                Parents = s.ParentStudents.Select(ps => new
                {
                    ps.Parent.UserId,
                    FullName = ps.Parent.User.FullName,
                    PhoneNumber = ps.Parent.User.PhoneNumber
                })
            }));
        }

        // ════════════════════════════════════════════════════════════
        // SUB-ADMINS
        // ════════════════════════════════════════════════════════════

        [HttpGet("subadmins")]
        public async Task<IActionResult> SubAdmins([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1)
        {
            RequirePermission("subadmin.view");
            var roleId = await _db.Roles.Where(r => r.RoleName == "BusCoordinator").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users
                .Include(u => u.SubAdminPermissions).ThenInclude(p => p.Permission)
                .Where(u => u.RoleId == roleId);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
            if (status == "Active") q = q.Where(u => u.IsActive);
            if (status == "Inactive") q = q.Where(u => !u.IsActive);

            var total = await q.CountAsync();
            const int ps = 20;
            var items = await q.OrderBy(u => u.FullName).Skip((page - 1) * ps).Take(ps)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    u.IsActive,
                    u.CreatedAt,
                    u.ProfileImageUrl,                         // ← NEW
                    Permissions = u.SubAdminPermissions.Select(p => p.Permission.PermissionKey).ToList()
                }).ToListAsync();

            return Ok(ApiResponse<object>.Ok(new { items, total, page, pageSize = ps }));
        }

        [HttpGet("subadmins/{id:int}")]
        public async Task<IActionResult> SubAdminById(int id)
        {
            RequirePermission("subadmin.view");
            var u = await _db.Users
                .Include(x => x.SubAdminPermissions).ThenInclude(p => p.Permission)
                .FirstOrDefaultAsync(x => x.UserId == id);
            if (u is null) return NotFound(ApiResponse<object>.Fail("Sub-admin not found."));
            return Ok(ApiResponse<object>.Ok(new
            {
                u.UserId,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.IsActive,
                u.CreatedAt,
                u.ProfileImageUrl,                             // ← NEW
                Permissions = u.SubAdminPermissions.Select(p => p.Permission.PermissionKey).ToList(),
                PermissionIds = u.SubAdminPermissions.Select(p => p.PermissionId).ToList()
            }));
        }

        [HttpGet("subadmins/{id:int}/permissions")]
        public async Task<IActionResult> SubAdminPermissions(int id)
        {
            RequirePermission("subadmin.view");
            var allPerms = await _db.Permissions.OrderBy(p => p.ModuleName).ToListAsync();
            var assigned = await _db.SubAdminPermissions.Where(p => p.UserId == id).Select(p => p.PermissionId).ToListAsync();
            return Ok(ApiResponse<object>.Ok(new
            {
                allPermissions = allPerms.Select(p => new { p.PermissionId, p.ModuleName, p.PermissionKey, p.Description }),
                assignedPermissionIds = assigned
            }));
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> AllPermissions()
        {
            RequirePermission("subadmin.view");
            var perms = await _db.Permissions.OrderBy(p => p.ModuleName).ThenBy(p => p.PermissionKey)
                .Select(p => new { p.PermissionId, p.ModuleName, p.PermissionKey, p.Description })
                .ToListAsync();
            return Ok(ApiResponse<object>.Ok(perms));
        }

        [HttpPost("subadmins")]
        public async Task<IActionResult> CreateSubAdmin([FromBody] CreateSubAdminDto dto)
        {
            RequirePermission("subadmin.add");
            var r = await _subAdmin.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPut("subadmins/{id:int}")]
        public async Task<IActionResult> UpdateSubAdmin(int id, [FromBody] UpdateSubAdminDto dto)
        {
            RequirePermission("subadmin.edit");
            if (id == CurrentUserId) return BadRequest(ApiResponse<object>.Fail("Cannot edit your own account."));
            var r = await _subAdmin.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("subadmins/{id:int}")]
        public async Task<IActionResult> DeleteSubAdmin(int id)
        {
            RequirePermission("subadmin.delete");
            if (id == CurrentUserId) return BadRequest(ApiResponse<object>.Fail("Cannot delete your own account."));
            var r = await _subAdmin.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("subadmins/{id:int}/toggle")]
        public async Task<IActionResult> ToggleSubAdmin(int id)
        {
            RequirePermission("subadmin.edit");
            if (id == CurrentUserId) return BadRequest(ApiResponse<object>.Fail("Cannot toggle your own account."));
            var r = await _subAdmin.ToggleActiveAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("subadmins/{id:int}/reset-password")]
        public async Task<IActionResult> ResetSubAdminPassword(int id)
        {
            RequirePermission("subadmin.edit");
            if (id == CurrentUserId) return BadRequest(ApiResponse<object>.Fail("Cannot reset your own password here."));
            var r = await _subAdmin.ResetPasswordAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        // ════════════════════════════════════════════════════════════
        // APP CONFIG
        // ════════════════════════════════════════════════════════════

        [HttpGet("config")]
        public async Task<IActionResult> GetConfigs([FromQuery] string? platform, [FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] int page = 1)
        {
            RequirePermission("appconfig.view");
            var r = await _appConfig.GetAllAsync(platform, search, isActive, page);
            return Ok(r);
        }

        [HttpGet("config/{id:int}")]
        public async Task<IActionResult> GetConfigById(int id)
        {
            RequirePermission("appconfig.view");
            var r = await _appConfig.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        [HttpPost("config")]
        public async Task<IActionResult> CreateConfig([FromBody] CreateAppConfigDto dto)
        {
            RequirePermission("appconfig.add");
            var r = await _appConfig.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPut("config/{id:int}")]
        public async Task<IActionResult> UpdateConfig(int id, [FromBody] UpdateAppConfigDto dto)
        {
            RequirePermission("appconfig.edit");
            var r = await _appConfig.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("config/{id:int}")]
        public async Task<IActionResult> DeleteConfig(int id)
        {
            RequirePermission("appconfig.delete");
            var r = await _appConfig.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("config/{id:int}/toggle")]
        public async Task<IActionResult> ToggleConfig(int id)
        {
            RequirePermission("appconfig.edit");
            var r = await _appConfig.ToggleActiveAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        // ════════════════════════════════════════════════════════════
        // FEEDBACK
        // ════════════════════════════════════════════════════════════

        [HttpGet("feedback")]
        public async Task<IActionResult> GetFeedback([FromQuery] int page = 1, [FromQuery] string? status = null)
        {
            RequirePermission("helpsupport.view");
            var r = await _feedback.GetAllAsync(page, 20, status);
            return Ok(r);
        }

        [HttpGet("feedback/{id:int}")]
        public async Task<IActionResult> GetFeedbackById(int id)
        {
            RequirePermission("helpsupport.view");
            var all = await _feedback.GetAllAsync(1, 1000, null);
            var item = all.Data?.Items?.FirstOrDefault(f => f.FeedbackId == id);
            return item is not null
                ? Ok(ApiResponse<FeedbackListDto>.Ok(item))
                : NotFound(ApiResponse<FeedbackListDto>.Fail("Feedback not found."));
        }

        [HttpPut("feedback/{id:int}/status")]
        public async Task<IActionResult> UpdateFeedbackStatus(int id, [FromBody] UpdateFeedbackStatusRequest req)
        {
            RequirePermission("helpsupport.manage");
            var r = await _feedback.UpdateStatusAsync(id, req.Status, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        public class UpdateFeedbackStatusRequest { public string Status { get; set; } = ""; }

        // ════════════════════════════════════════════════════════════
        // NOTIFICATIONS
        // ════════════════════════════════════════════════════════════

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            RequirePermission("notification.manage");
            var r = await _notif.GetUserNotificationsAsync(CurrentUserId);
            return Ok(r);
        }

        [HttpPost("notifications/{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            RequirePermission("notification.manage");
            var r = await _notif.MarkAsReadAsync(id, CurrentUserId);
            return Ok(r);
        }

        [HttpPost("notifications/read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            RequirePermission("notification.manage");
            var r = await _notif.MarkAllAsReadAsync(CurrentUserId);
            return Ok(r);
        }


        private void RequirePermission(string key)
        {
            if (!User.HasClaim("permission", key))
                throw new UnauthorizedAccessException($"Missing permission: {key}");
        }
    }
}
