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
        private readonly IBusService _bus;
        private readonly IDriverService _driver;
        private readonly IParentService _parent;
        private readonly IStudentService _student;

        public CoordinatorController(AppDbContext db, IDashboardService dash, ITripService trip,
            ISubAdminService subAdmin, IAppConfigService appConfig, IFeedbackService feedback,
            INotificationService notif, IImageService img, IRouteService route, IBusService bus, IDriverService driver, IParentService parent, IStudentService student)
        {
            _db = db; _dash = dash; _trip = trip;
            _subAdmin = subAdmin; _appConfig = appConfig;
            _feedback = feedback; _notif = notif; _img = img; _route = route; _bus = bus; _driver = driver; _parent = parent; _student = student;
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
        public async Task<IActionResult> Trips([FromQuery] int page = 1, [FromQuery] string? status = null, [FromQuery] string? date = null)
        {
            var effectiveDate = DateOnly.TryParse(date, out _) ? date : DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
            var r = await _trip.GetAllAsync(page, null, status, effectiveDate);
            return Ok(r);
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
        public async Task<IActionResult> Buses([FromQuery] int page = 1, [FromQuery] string? search = null, [FromQuery] string? status = "Active")
        {
            var r = await _bus.GetAllAsync(page, search, status);
            return Ok(r);
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

        [HttpGet("routes/{id}")]
        public async Task<IActionResult> GetRoute(int id)
        {
            var r = await _route.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        [HttpPut("routes/{id}")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] UpdateRouteDto dto)
        {
            RequirePermission("route.edit");
            var r = await _route.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("routes/{id}/stops")]
        public async Task<IActionResult> AddStop(int id, [FromBody] CreateStopDto dto)
        {
            RequirePermission("route.edit");
            dto.RouteId = id;
            var r = await _route.AddStopAsync(dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("routes/stops/{stopId}")]
        public async Task<IActionResult> DeleteStop(int stopId)
        {
            RequirePermission("route.edit");
            var r = await _route.DeleteStopAsync(stopId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpGet("routes/{routeId}/stops")]
        public async Task<IActionResult> RouteStops(int routeId)
        {
            var r = await _route.GetStopsByRouteAsync(routeId);
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // DRIVERS
        // ════════════════════════════════════════════════════════════

        [HttpGet("drivers")]
        public async Task<IActionResult> Drivers([FromQuery] int page = 1, [FromQuery] string? search = null, [FromQuery] string? status = "Active")
        {
            var r = await _driver.GetAllAsync(page, search, status);
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // PARENTS
        // ════════════════════════════════════════════════════════════

        [HttpGet("parents")]
        public async Task<IActionResult> Parents([FromQuery] int page = 1, [FromQuery] string? search = null, [FromQuery] string? status = "Active")
        {
            var r = await _parent.GetAllAsync(page, search, status);
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // STUDENTS
        // ════════════════════════════════════════════════════════════

        [HttpGet("students")]
        public async Task<IActionResult> Students([FromQuery] int page = 1, [FromQuery] string? search = null, [FromQuery] string? status = "Active")
        {
            var r = await _student.GetAllAsync(page, search, status);
            return Ok(r);
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
            var r = await _subAdmin.GetAllAsync(page, search, status);
            return Ok(r);
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
            // SuperAdmin always has full access — skip permission claim check
            if (User.IsInRole("SuperAdmin")) return;

            if (!User.HasClaim("permission", key))
                throw new UnauthorizedAccessException($"Missing permission: {key}");
        }
    }
}
