namespace BusTracking.API.Controllers
{
    /// <summary>
    /// Full SuperAdmin API for MAUI app.
    /// All endpoints require role = SuperAdmin.
    /// Base route: /api/admin
    /// </summary>
    [Authorize(Roles = "SuperAdmin"), Route("api/admin")]
    public class SuperAdminController : ApiBaseController
    {
        private readonly IBusService      _bus;
        private readonly IRouteService    _route;
        private readonly IDriverService   _driver;
        private readonly IStudentService  _student;
        private readonly IParentService   _parent;
        private readonly ISubAdminService _subAdmin;
        private readonly ITripService     _trip;
        private readonly IFeedbackService _feedback;
        private readonly INotificationService _notif;
        private readonly IDashboardService    _dash;
        private readonly IAppConfigService    _config;
        private readonly AppDbContext         _db;

        public SuperAdminController(
            IBusService bus, IRouteService route, IDriverService driver,
            IStudentService student, IParentService parent, ISubAdminService subAdmin,
            ITripService trip, IFeedbackService feedback, INotificationService notif,
            IDashboardService dash, IAppConfigService config, AppDbContext db)
        {
            _bus = bus; _route = route; _driver = driver; _student = student;
            _parent = parent; _subAdmin = subAdmin; _trip = trip; _feedback = feedback;
            _notif = notif; _dash = dash; _config = config; _db = db;
        }

        // ════════════════════════════════════════════════════════════
        // DASHBOARD
        // ════════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/dashboard — System summary counts</summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var r = await _dash.GetSummaryAsync();
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // BUS COORDINATORS (SubAdmins)
        // ════════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/coordinators?page=1&search=&status=</summary>
        [HttpGet("coordinators")]
        public async Task<IActionResult> GetCoordinators([FromQuery] int page = 1,
            [FromQuery] string? search = null, [FromQuery] string? status = null)
        {
            var r = await _subAdmin.GetAllAsync(page, 20, search, status);
            return Ok(r);
        }

        /// <summary>GET /api/admin/coordinators/{id}</summary>
        [HttpGet("coordinators/{id}")]
        public async Task<IActionResult> GetCoordinator(int id)
        {
            var r = await _subAdmin.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        /// <summary>POST /api/admin/coordinators — Create bus coordinator</summary>
        [HttpPost("coordinators")]
        public async Task<IActionResult> CreateCoordinator([FromBody] CreateSubAdminDto dto)
        {
            var r = await _subAdmin.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>PUT /api/admin/coordinators/{id} — Update coordinator + permissions</summary>
        [HttpPut("coordinators/{id}")]
        public async Task<IActionResult> UpdateCoordinator(int id, [FromBody] UpdateSubAdminDto dto)
        {
            var r = await _subAdmin.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>DELETE /api/admin/coordinators/{id}</summary>
        [HttpDelete("coordinators/{id}")]
        public async Task<IActionResult> DeleteCoordinator(int id)
        {
            var r = await _subAdmin.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>POST /api/admin/coordinators/{id}/toggle — Toggle active/inactive</summary>
        [HttpPost("coordinators/{id}/toggle")]
        public async Task<IActionResult> ToggleCoordinator(int id)
        {
            var r = await _subAdmin.ToggleActiveAsync(id);
            return Ok(r);
        }

        /// <summary>POST /api/admin/coordinators/{id}/reset-password</summary>
        [HttpPost("coordinators/{id}/reset-password")]
        public async Task<IActionResult> ResetCoordinatorPassword(int id)
        {
            var r = await _subAdmin.ResetPasswordAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>GET /api/admin/coordinators/{id}/permissions</summary>
        [HttpGet("coordinators/{id}/permissions")]
        public async Task<IActionResult> GetCoordinatorPermissions(int id)
        {
            var assigned = await _subAdmin.GetPermissionIdsAsync(id);
            var all = await _subAdmin.GetAllPermissionsAsync();
            return Ok(ApiResponse<object>.Ok(new
            {
                AssignedPermissionIds = assigned,
                AllPermissions = all.Select(p => new { p.Id, p.ModuleName, p.Key, p.Description })
            }));
        }

        /// <summary>GET /api/admin/permissions — All available permissions</summary>
        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var all = await _subAdmin.GetAllPermissionsAsync();
            return Ok(ApiResponse<object>.Ok(
                all.Select(p => new { p.Id, p.ModuleName, p.Key, p.Description })
            ));
        }

        // ════════════════════════════════════════════════════════════
        // BUSES
        // ════════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/buses?page=1&search=&status=</summary>
        [HttpGet("buses")]
        public async Task<IActionResult> GetBuses([FromQuery] int page = 1,
            [FromQuery] string? search = null, [FromQuery] string? status = null)
        {
            var r = await _bus.GetAllAsync(page, 20, search, status);
            return Ok(r);
        }

        /// <summary>GET /api/admin/buses/{id}</summary>
        [HttpGet("buses/{id}")]
        public async Task<IActionResult> GetBus(int id)
        {
            var r = await _bus.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        /// <summary>POST /api/admin/buses — Create bus</summary>
        [HttpPost("buses")]
        public async Task<IActionResult> CreateBus([FromBody] CreateBusDto dto)
        {
            var r = await _bus.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>PUT /api/admin/buses/{id} — Update bus</summary>
        [HttpPut("buses/{id}")]
        public async Task<IActionResult> UpdateBus(int id, [FromBody] UpdateBusDto dto)
        {
            var r = await _bus.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>DELETE /api/admin/buses/{id}</summary>
        [HttpDelete("buses/{id}")]
        public async Task<IActionResult> DeleteBus(int id)
        {
            var r = await _bus.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>POST /api/admin/buses/{id}/toggle</summary>
        [HttpPost("buses/{id}/toggle")]
        public async Task<IActionResult> ToggleBus(int id)
        {
            var r = await _bus.ToggleActiveAsync(id);
            return Ok(r);
        }

        /// <summary>POST /api/admin/buses/{id}/assign-driver — Assign driver to bus</summary>
        [HttpPost("buses/{id}/assign-driver")]
        public async Task<IActionResult> AssignDriver(int id, [FromBody] AssignDriverRequest req)
        {
            var r = await _bus.AssignDriverAsync(new AssignDriverToBusDto
            {
                BusId = id,
                DriverUserId = req.DriverUserId
            });
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>GET /api/admin/buses/dropdown?search= — For dropdowns in mobile form</summary>
        [HttpGet("buses/dropdown")]
        public async Task<IActionResult> BusDropdown([FromQuery] string? search)
        {
            var r = await _bus.GetDropdownAsync(search);
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // ROUTES
        // ════════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/routes?page=1&search=</summary>
        [HttpGet("routes")]
        public async Task<IActionResult> GetRoutes([FromQuery] int page = 1,
            [FromQuery] string? search = null)
        {
            var r = await _route.GetAllAsync(page, 20, search);
            return Ok(r);
        }

        /// <summary>GET /api/admin/routes/{id} — Route detail with stops</summary>
        [HttpGet("routes/{id}")]
        public async Task<IActionResult> GetRoute(int id)
        {
            var r = await _route.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        /// <summary>POST /api/admin/routes — Create route</summary>
        [HttpPost("routes")]
        public async Task<IActionResult> CreateRoute([FromBody] CreateRouteDto dto)
        {
            var r = await _route.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>PUT /api/admin/routes/{id} — Update route</summary>
        [HttpPut("routes/{id}")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] UpdateRouteDto dto)
        {
            var r = await _route.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>DELETE /api/admin/routes/{id}</summary>
        [HttpDelete("routes/{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var r = await _route.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>GET /api/admin/routes/{id}/stops</summary>
        [HttpGet("routes/{id}/stops")]
        public async Task<IActionResult> GetRouteStops(int id)
        {
            var r = await _route.GetStopsByRouteAsync(id);
            return Ok(r);
        }

        /// <summary>POST /api/admin/routes/{id}/stops — Add stop to route</summary>
        [HttpPost("routes/{id}/stops")]
        public async Task<IActionResult> AddStop(int id, [FromBody] CreateStopDto dto)
        {
            dto.RouteId = id;
            var r = await _route.AddStopAsync(dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>DELETE /api/admin/routes/stops/{stopId}</summary>
        [HttpDelete("routes/stops/{stopId}")]
        public async Task<IActionResult> DeleteStop(int stopId)
        {
            var r = await _route.DeleteStopAsync(stopId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        // ════════════════════════════════════════════════════════════
        // DRIVERS
        // ════════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/drivers?page=1&search=&status=</summary>
        [HttpGet("drivers")]
        public async Task<IActionResult> GetDrivers([FromQuery] int page = 1,
            [FromQuery] string? search = null, [FromQuery] string? status = null)
        {
            var r = await _driver.GetAllAsync(page, 20, search, status);
            return Ok(r);
        }

        /// <summary>GET /api/admin/drivers/{id}</summary>
        [HttpGet("drivers/{id}")]
        public async Task<IActionResult> GetDriver(int id)
        {
            var r = await _driver.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        /// <summary>POST /api/admin/drivers — Create driver</summary>
        [HttpPost("drivers")]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto dto)
        {
            var r = await _driver.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>PUT /api/admin/drivers/{id} — Update driver</summary>
        [HttpPut("drivers/{id}")]
        public async Task<IActionResult> UpdateDriver(int id, [FromBody] UpdateDriverDto dto)
        {
            var r = await _driver.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>DELETE /api/admin/drivers/{id}</summary>
        [HttpDelete("drivers/{id}")]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            var r = await _driver.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>POST /api/admin/drivers/{id}/toggle</summary>
        [HttpPost("drivers/{id}/toggle")]
        public async Task<IActionResult> ToggleDriver(int id)
        {
            var r = await _driver.ToggleActiveAsync(id);
            return Ok(r);
        }

        /// <summary>POST /api/admin/drivers/{id}/reset-password</summary>
        [HttpPost("drivers/{id}/reset-password")]
        public async Task<IActionResult> ResetDriverPassword(int id)
        {
            var r = await _driver.ResetPasswordAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>GET /api/admin/drivers/dropdown?search=</summary>
        [HttpGet("drivers/dropdown")]
        public async Task<IActionResult> DriverDropdown([FromQuery] string? search)
        {
            var r = await _driver.GetDropdownAsync(search);
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // PARENTS
        // ════════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/parents?page=1&search=&status=</summary>
        [HttpGet("parents")]
        public async Task<IActionResult> GetParents([FromQuery] int page = 1,
            [FromQuery] string? search = null, [FromQuery] string? status = null)
        {
            var r = await _parent.GetAllAsync(page, 20, search, status);
            return Ok(r);
        }

        /// <summary>GET /api/admin/parents/{id}</summary>
        [HttpGet("parents/{id}")]
        public async Task<IActionResult> GetParent(int id)
        {
            var r = await _parent.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        /// <summary>POST /api/admin/parents — Create parent</summary>
        [HttpPost("parents")]
        public async Task<IActionResult> CreateParent([FromBody] CreateParentDto dto)
        {
            var r = await _parent.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>PUT /api/admin/parents/{id} — Update parent + linked students</summary>
        [HttpPut("parents/{id}")]
        public async Task<IActionResult> UpdateParent(int id, [FromBody] UpdateParentDto dto)
        {
            var r = await _parent.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>DELETE /api/admin/parents/{id}</summary>
        [HttpDelete("parents/{id}")]
        public async Task<IActionResult> DeleteParent(int id)
        {
            var r = await _parent.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>POST /api/admin/parents/{id}/toggle</summary>
        [HttpPost("parents/{id}/toggle")]
        public async Task<IActionResult> ToggleParent(int id)
        {
            var r = await _parent.ToggleActiveAsync(id);
            return Ok(r);
        }

        /// <summary>POST /api/admin/parents/{id}/reset-password</summary>
        [HttpPost("parents/{id}/reset-password")]
        public async Task<IActionResult> ResetParentPassword(int id)
        {
            var r = await _parent.ResetPasswordAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        // ════════════════════════════════════════════════════════════
        // STUDENTS
        // ════════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/students?page=1&search=&status=</summary>
        [HttpGet("students")]
        public async Task<IActionResult> GetStudents([FromQuery] int page = 1,
            [FromQuery] string? search = null, [FromQuery] string? status = null)
        {
            var r = await _student.GetAllAsync(page, 20, search, status);
            return Ok(r);
        }

        /// <summary>GET /api/admin/students/{id}</summary>
        [HttpGet("students/{id}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            var r = await _student.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        /// <summary>POST /api/admin/students — Create student</summary>
        [HttpPost("students")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto dto)
        {
            var r = await _student.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>PUT /api/admin/students/{id} — Update student</summary>
        [HttpPut("students/{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto dto)
        {
            var r = await _student.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>DELETE /api/admin/students/{id}</summary>
        [HttpDelete("students/{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var r = await _student.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>POST /api/admin/students/{id}/toggle</summary>
        [HttpPost("students/{id}/toggle")]
        public async Task<IActionResult> ToggleStudent(int id)
        {
            var r = await _student.ToggleActiveAsync(id);
            return Ok(r);
        }

        /// <summary>POST /api/admin/students/{id}/reset-password</summary>
        [HttpPost("students/{id}/reset-password")]
        public async Task<IActionResult> ResetStudentPassword(int id)
        {
            var r = await _student.ResetPasswordAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>GET /api/admin/students/{id}/availability</summary>
        [HttpGet("students/{id}/availability")]
        public async Task<IActionResult> GetStudentAvailability(int id)
        {
            var r = await _student.GetAvailabilitiesAsync(id);
            return Ok(r);
        }

        /// <summary>POST /api/admin/students/{id}/availability</summary>
        [HttpPost("students/{id}/availability")]
        public async Task<IActionResult> SetStudentAvailability(int id,
            [FromBody] CreateAvailabilityDto dto)
        {
            dto.StudentId = id;
            var r = await _student.SetAvailabilityAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>GET /api/admin/students/search?query= — Search by name/code</summary>
        [HttpGet("students/search")]
        public async Task<IActionResult> SearchStudents([FromQuery] string? query)
        {
            var r = await _student.SearchAsync(query);
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // TRIPS
        // ════════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/trips?page=1&busId=</summary>
        [HttpGet("trips")]
        public async Task<IActionResult> GetTrips([FromQuery] int page = 1,
            [FromQuery] string? busId = null)
        {
            var r = await _trip.GetAllAsync(page, 20, busId);
            return Ok(r);
        }

        /// <summary>GET /api/admin/trips/{id}</summary>
        [HttpGet("trips/{id}")]
        public async Task<IActionResult> GetTrip(int id)
        {
            var r = await _trip.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        /// <summary>POST /api/admin/trips — Create trip</summary>
        [HttpPost("trips")]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDto dto)
        {
            var r = await _trip.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>POST /api/admin/trips/{id}/start</summary>
        [HttpPost("trips/{id}/start")]
        public async Task<IActionResult> StartTrip(int id)
        {
            var r = await _trip.StartTripAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>POST /api/admin/trips/{id}/end</summary>
        [HttpPost("trips/{id}/end")]
        public async Task<IActionResult> EndTrip(int id)
        {
            var r = await _trip.EndTripAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>POST /api/admin/trips/{id}/cancel</summary>
        [HttpPost("trips/{id}/cancel")]
        public async Task<IActionResult> CancelTrip(int id)
        {
            var r = await _trip.CancelTripAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>DELETE /api/admin/trips/{id}</summary>
        [HttpDelete("trips/{id}")]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            var r = await _trip.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>GET /api/admin/trips/{id}/students</summary>
        [HttpGet("trips/{id}/students")]
        public async Task<IActionResult> TripStudents(int id)
        {
            var r = await _trip.GetTripStudentsAsync(id);
            return Ok(r);
        }

        /// <summary>GET /api/admin/trips/{id}/stops</summary>
        [HttpGet("trips/{id}/stops")]
        public async Task<IActionResult> TripStops(int id)
        {
            var r = await _trip.GetStopEventsAsync(id);
            return Ok(r);
        }

        /// <summary>POST /api/admin/trips/{id}/stops/{stopId}/reach</summary>
        [HttpPost("trips/{id}/stops/{stopId}/reach")]
        public async Task<IActionResult> ReachStop(int id, int stopId)
        {
            var r = await _trip.ReachStopAsync(id, stopId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>PUT /api/admin/trips/{id}/boarding — Update student boarding status</summary>
        [HttpPut("trips/{id}/boarding")]
        public async Task<IActionResult> UpdateBoarding(int id, [FromBody] UpdateBoardingRequest req)
        {
            var r = await _trip.UpdateBoardingAsync(id, req.StudentId, req.StopId, req.Status);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>GET /api/admin/trips/{id}/location — Latest GPS location</summary>
        [HttpGet("trips/{id}/location")]
        public async Task<IActionResult> TripLocation(int id)
        {
            var r = await _trip.GetLatestLocationAsync(id);
            return r.Data is not null ? Ok(r) : NotFound(ApiResponse<object>.Fail("No location data yet."));
        }

        /// <summary>GET /api/admin/trips/{id}/location/history — Full GPS trail</summary>
        [HttpGet("trips/{id}/location/history")]
        public async Task<IActionResult> TripLocationHistory(int id)
        {
            var r = await _trip.GetLocationHistoryAsync(id);
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // FEEDBACK / SUPPORT
        // ════════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/feedback?page=1&status=</summary>
        [HttpGet("feedback")]
        public async Task<IActionResult> GetFeedback([FromQuery] int page = 1,
            [FromQuery] string? status = null)
        {
            var r = await _feedback.GetAllAsync(page, 20, status);
            return Ok(r);
        }

        /// <summary>PUT /api/admin/feedback/{id}/status — Resolve or close feedback</summary>
        [HttpPut("feedback/{id}/status")]
        public async Task<IActionResult> UpdateFeedbackStatus(int id,
            [FromBody] UpdateFeedbackStatusRequest req)
        {
            var r = await _feedback.UpdateStatusAsync(id, req.Status, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        // ════════════════════════════════════════════════════════════
        // NOTIFICATIONS
        // ════════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/notifications</summary>
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var r = await _notif.GetUserNotificationsAsync(CurrentUserId);
            return Ok(r);
        }

        /// <summary>POST /api/admin/notifications/send — Send notification to a user</summary>
        [HttpPost("notifications/send")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest req)
        {
            await _notif.SendAsync(req.RecipientUserId, req.Title, req.Body, req.Type);
            return Ok(ApiResponse<bool>.Ok(true, "Notification sent."));
        }

        /// <summary>PUT /api/admin/notifications/{id}/read</summary>
        [HttpPut("notifications/{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var r = await _notif.MarkAsReadAsync(id, CurrentUserId);
            return Ok(r);
        }

        /// <summary>PUT /api/admin/notifications/read-all</summary>
        [HttpPut("notifications/read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var r = await _notif.MarkAllAsReadAsync(CurrentUserId);
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // APP CONFIGURATION
        // ════════════════════════════════════════════════════════════

        /// <summary>GET /api/admin/config?platform=&search=&isActive= — List all configs</summary>
        [HttpGet("config")]
        public async Task<IActionResult> GetConfigs([FromQuery] string? platform,
            [FromQuery] string? search, [FromQuery] bool? isActive)
        {
            var r = await _config.GetAllAsync(platform, search, isActive);
            return Ok(r);
        }

        /// <summary>GET /api/admin/config/{id}</summary>
        [HttpGet("config/{id}")]
        public async Task<IActionResult> GetConfig(int id)
        {
            var r = await _config.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        /// <summary>POST /api/admin/config — Create config key</summary>
        [HttpPost("config")]
        public async Task<IActionResult> CreateConfig([FromBody] CreateAppConfigDto dto)
        {
            var r = await _config.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>PUT /api/admin/config/{id} — Update config key</summary>
        [HttpPut("config/{id}")]
        public async Task<IActionResult> UpdateConfig(int id, [FromBody] UpdateAppConfigDto dto)
        {
            var r = await _config.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>DELETE /api/admin/config/{id}</summary>
        [HttpDelete("config/{id}")]
        public async Task<IActionResult> DeleteConfig(int id)
        {
            var r = await _config.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>POST /api/admin/config/{id}/toggle — Toggle active/inactive</summary>
        [HttpPost("config/{id}/toggle")]
        public async Task<IActionResult> ToggleConfig(int id)
        {
            var r = await _config.ToggleActiveAsync(id);
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // REQUEST MODELS
        // ════════════════════════════════════════════════════════════

        public class AssignDriverRequest
        {
            public int DriverUserId { get; set; }
        }

        public class UpdateBoardingRequest
        {
            public int StudentId { get; set; }
            public int StopId { get; set; }
            public string Status { get; set; } = "";   // Pending | PickedUp | NoShow | OnLeave
        }

        public class UpdateFeedbackStatusRequest
        {
            public string Status { get; set; } = "";   // Open | InProgress | Resolved | Closed
        }

        public class SendNotificationRequest
        {
            public int RecipientUserId { get; set; }
            public string Title { get; set; } = "";
            public string Body { get; set; } = "";
            public string Type { get; set; } = "General";
        }
    }
}
