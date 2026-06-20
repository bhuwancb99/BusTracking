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
        private readonly IBusService _bus;
        private readonly IRouteService _route;
        private readonly IDriverService _driver;
        private readonly IStudentService _student;
        private readonly IParentService _parent;
        private readonly ISubAdminService _subAdmin;
        private readonly ITripService _trip;
        private readonly IFeedbackService _feedback;
        private readonly INotificationService _notif;
        private readonly IDashboardService _dash;
        private readonly IAppConfigService _config;
        private readonly AppDbContext _db;
        private readonly IImageService _img;         // ← NEW

        private const int MAX_BUS_IMAGES = 5;               // ← NEW

        public SuperAdminController(
            IBusService bus, IRouteService route, IDriverService driver,
            IStudentService student, IParentService parent, ISubAdminService subAdmin,
            ITripService trip, IFeedbackService feedback, INotificationService notif,
            IDashboardService dash, IAppConfigService config, AppDbContext db,
            IImageService img)                               // ← NEW
        {
            _bus = bus; _route = route; _driver = driver; _student = student;
            _parent = parent; _subAdmin = subAdmin; _trip = trip; _feedback = feedback;
            _notif = notif; _dash = dash; _config = config; _db = db; _img = img;
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

        // ════════════════════════════════════════════════════════════
        // BUS COORDINATORS
        // ════════════════════════════════════════════════════════════

        [HttpGet("coordinators")]
        public async Task<IActionResult> GetCoordinators([FromQuery] int page = 1,
            [FromQuery] string? search = null, [FromQuery] string? status = null)
        {
            var r = await _subAdmin.GetAllAsync(page, 20, search, status);
            return Ok(r);
        }

        [HttpGet("coordinators/{id}")]
        public async Task<IActionResult> GetCoordinator(int id)
        {
            var r = await _subAdmin.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        [HttpPost("coordinators")]
        public async Task<IActionResult> CreateCoordinator([FromBody] CreateSubAdminDto dto)
        {
            var r = await _subAdmin.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPut("coordinators/{id}")]
        public async Task<IActionResult> UpdateCoordinator(int id, [FromBody] UpdateSubAdminDto dto)
        {
            var r = await _subAdmin.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("coordinators/{id}")]
        public async Task<IActionResult> DeleteCoordinator(int id)
        {
            var r = await _subAdmin.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("coordinators/{id}/toggle")]
        public async Task<IActionResult> ToggleCoordinator(int id)
        {
            var r = await _subAdmin.ToggleActiveAsync(id);
            return Ok(r);
        }

        [HttpPost("coordinators/{id}/reset-password")]
        public async Task<IActionResult> ResetCoordinatorPassword(int id)
        {
            var r = await _subAdmin.ResetPasswordAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

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

        /// <summary>POST /api/admin/coordinators/{id}/photo — Upload coordinator profile photo</summary>
        [HttpPost("coordinators/{id}/photo")]
        [RequestSizeLimit(5_242_880)]
        public async Task<IActionResult> UploadCoordinatorPhoto(int id, IFormFile file)
            => await UploadUserPhoto(id, "coordinator", file);

        /// <summary>DELETE /api/admin/coordinators/{id}/photo</summary>
        [HttpDelete("coordinators/{id}/photo")]
        public async Task<IActionResult> DeleteCoordinatorPhoto(int id)
            => await DeleteUserPhoto(id);

        // ════════════════════════════════════════════════════════════
        // PERMISSIONS
        // ════════════════════════════════════════════════════════════

        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var all = await _subAdmin.GetAllPermissionsAsync();
            return Ok(ApiResponse<object>.Ok(
                all.Select(p => new { p.Id, p.ModuleName, p.Key, p.Description })));
        }

        // ════════════════════════════════════════════════════════════
        // BUSES
        // ════════════════════════════════════════════════════════════

        [HttpGet("buses")]
        public async Task<IActionResult> GetBuses([FromQuery] int page = 1,
            [FromQuery] string? search = null, [FromQuery] string? status = null)
        {
            var r = await _bus.GetAllAsync(page, 20, search, status);
            return Ok(r);
        }

        [HttpGet("buses/{id}")]
        public async Task<IActionResult> GetBus(int id)
        {
            var r = await _bus.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        [HttpPost("buses")]
        public async Task<IActionResult> CreateBus([FromBody] CreateBusDto dto)
        {
            var r = await _bus.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPut("buses/{id}")]
        public async Task<IActionResult> UpdateBus(int id, [FromBody] UpdateBusDto dto)
        {
            var r = await _bus.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("buses/{id}")]
        public async Task<IActionResult> DeleteBus(int id)
        {
            var r = await _bus.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("buses/{id}/toggle")]
        public async Task<IActionResult> ToggleBus(int id)
        {
            var r = await _bus.ToggleActiveAsync(id);
            return Ok(r);
        }

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

        [HttpGet("buses/dropdown")]
        public async Task<IActionResult> BusDropdown([FromQuery] string? search)
        {
            var r = await _bus.GetDropdownAsync(search);
            return Ok(r);
        }

        /// <summary>
        /// POST /api/admin/buses/{id}/images
        /// Upload 1–5 images for a bus. Send as multipart/form-data, field name "files".
        /// Returns per-file results.
        /// </summary>
        [HttpPost("buses/{id}/images")]
        [RequestSizeLimit(26_214_400)] // 5 × 5 MB
        public async Task<IActionResult> UploadBusImages(int id, [FromForm] List<IFormFile> files)
        {
            var bus = await _db.Buses.Include(b => b.Images).FirstOrDefaultAsync(b => b.BusId == id);
            if (bus is null) return NotFound(ApiResponse<object>.Fail("Bus not found."));

            int existing = bus.Images.Count;
            int remaining = MAX_BUS_IMAGES - existing;

            if (remaining <= 0)
                return BadRequest(ApiResponse<object>.Fail(
                    $"This bus already has {MAX_BUS_IMAGES} photos. Delete one to upload more."));

            if (files.Count > remaining)
                return BadRequest(ApiResponse<object>.Fail(
                    $"Only {remaining} more photo{(remaining == 1 ? "" : "s")} allowed (limit is {MAX_BUS_IMAGES})."));

            var uploaded = new List<object>();
            var failed = new List<string>();
            int nextIndex = bus.Images.Any() ? bus.Images.Max(i => ExtractIndex(i.ImageUrl)) + 1 : 1;

            foreach (var file in files)
            {
                try
                {
                    var url = await _img.SaveBusImageAsync(file, id, nextIndex);
                    bool isFirst = !bus.Images.Any() && uploaded.Count == 0;

                    var busImage = new BusImage
                    {
                        BusId = id,
                        ImageUrl = url,
                        DisplayOrder = nextIndex,
                        IsPrimary = isFirst,
                        UploadedBy = CurrentUserId
                    };
                    _db.BusImages.Add(busImage);
                    await _db.SaveChangesAsync();
                    bus.Images.Add(busImage);
                    nextIndex++;

                    uploaded.Add(new
                    {
                        busImageId = busImage.BusImageId,
                        imageUrl = url,
                        isPrimary = busImage.IsPrimary
                    });
                }
                catch (InvalidOperationException ex) { failed.Add($"{file.FileName}: {ex.Message}"); }
            }

            int newTotal = existing + uploaded.Count;
            return Ok(ApiResponse<object>.Ok(new
            {
                uploaded,
                failed,
                totalNow = newTotal,
                remaining = MAX_BUS_IMAGES - newTotal,
                message = $"{uploaded.Count} photo(s) uploaded."
            }));
        }

        /// <summary>DELETE /api/admin/buses/images/{imageId} — Delete one bus image</summary>
        [HttpDelete("buses/images/{imageId}")]
        public async Task<IActionResult> DeleteBusImage(int imageId)
        {
            var img = await _db.BusImages
                .Include(i => i.Bus).ThenInclude(b => b.Images)
                .FirstOrDefaultAsync(i => i.BusImageId == imageId);

            if (img is null) return NotFound(ApiResponse<object>.Fail("Image not found."));

            _img.DeleteFile(img.ImageUrl);
            _db.BusImages.Remove(img);
            await _db.SaveChangesAsync();

            var remaining = img.Bus.Images
                .Where(i => i.BusImageId != imageId)
                .OrderBy(i => i.DisplayOrder).ToList();

            if (img.IsPrimary && remaining.Any())
            {
                remaining[0].IsPrimary = true;
                await _db.SaveChangesAsync();
            }

            return Ok(ApiResponse<object>.Ok(new
            {
                totalNow = remaining.Count,
                remaining = MAX_BUS_IMAGES - remaining.Count
            }, "Image deleted."));
        }

        /// <summary>POST /api/admin/buses/images/{imageId}/primary — Set cover image</summary>
        [HttpPost("buses/images/{imageId}/primary")]
        public async Task<IActionResult> SetPrimaryBusImage(int imageId)
        {
            var img = await _db.BusImages.FirstOrDefaultAsync(i => i.BusImageId == imageId);
            if (img is null) return NotFound(ApiResponse<object>.Fail("Image not found."));

            var others = await _db.BusImages.Where(i => i.BusId == img.BusId && i.IsPrimary).ToListAsync();
            foreach (var o in others) o.IsPrimary = false;
            img.IsPrimary = true;
            await _db.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok(true, "Cover image updated."));
        }

        // ════════════════════════════════════════════════════════════
        // ROUTES
        // ════════════════════════════════════════════════════════════

        [HttpGet("routes")]
        public async Task<IActionResult> GetRoutes([FromQuery] int page = 1, [FromQuery] string? search = null)
        {
            var r = await _route.GetAllAsync(page, 20, search);
            return Ok(r);
        }

        [HttpGet("routes/{id}")]
        public async Task<IActionResult> GetRoute(int id)
        {
            var r = await _route.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        [HttpPost("routes")]
        public async Task<IActionResult> CreateRoute([FromBody] CreateRouteDto dto)
        {
            var r = await _route.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPut("routes/{id}")]
        public async Task<IActionResult> UpdateRoute(int id, [FromBody] UpdateRouteDto dto)
        {
            var r = await _route.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("routes/{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var r = await _route.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpGet("routes/{id}/stops")]
        public async Task<IActionResult> GetRouteStops(int id)
        {
            var r = await _route.GetStopsByRouteAsync(id);
            return Ok(r);
        }

        [HttpPost("routes/{id}/stops")]
        public async Task<IActionResult> AddStop(int id, [FromBody] CreateStopDto dto)
        {
            dto.RouteId = id;
            var r = await _route.AddStopAsync(dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("routes/stops/{stopId}")]
        public async Task<IActionResult> DeleteStop(int stopId)
        {
            var r = await _route.DeleteStopAsync(stopId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        // ════════════════════════════════════════════════════════════
        // DRIVERS
        // ════════════════════════════════════════════════════════════

        [HttpGet("drivers")]
        public async Task<IActionResult> GetDrivers([FromQuery] int page = 1,
            [FromQuery] string? search = null, [FromQuery] string? status = null)
        {
            var r = await _driver.GetAllAsync(page, 20, search, status);
            return Ok(r);
        }

        [HttpGet("drivers/{id}")]
        public async Task<IActionResult> GetDriver(int id)
        {
            var r = await _driver.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        [HttpPost("drivers")]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto dto)
        {
            var r = await _driver.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPut("drivers/{id}")]
        public async Task<IActionResult> UpdateDriver(int id, [FromBody] UpdateDriverDto dto)
        {
            var r = await _driver.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("drivers/{id}")]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            var r = await _driver.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("drivers/{id}/toggle")]
        public async Task<IActionResult> ToggleDriver(int id)
        {
            var r = await _driver.ToggleActiveAsync(id);
            return Ok(r);
        }

        [HttpPost("drivers/{id}/reset-password")]
        public async Task<IActionResult> ResetDriverPassword(int id)
        {
            var r = await _driver.ResetPasswordAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpGet("drivers/dropdown")]
        public async Task<IActionResult> DriverDropdown([FromQuery] string? search)
        {
            var r = await _driver.GetDropdownAsync(search);
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // PARENTS
        // ════════════════════════════════════════════════════════════

        [HttpGet("parents")]
        public async Task<IActionResult> GetParents([FromQuery] int page = 1,
            [FromQuery] string? search = null, [FromQuery] string? status = null)
        {
            var r = await _parent.GetAllAsync(page, 20, search, status);
            return Ok(r);
        }

        [HttpGet("parents/{id}")]
        public async Task<IActionResult> GetParent(int id)
        {
            var r = await _parent.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        [HttpPost("parents")]
        public async Task<IActionResult> CreateParent([FromBody] CreateParentDto dto)
        {
            var r = await _parent.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPut("parents/{id}")]
        public async Task<IActionResult> UpdateParent(int id, [FromBody] UpdateParentDto dto)
        {
            var r = await _parent.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("parents/{id}")]
        public async Task<IActionResult> DeleteParent(int id)
        {
            var r = await _parent.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("parents/{id}/toggle")]
        public async Task<IActionResult> ToggleParent(int id)
        {
            var r = await _parent.ToggleActiveAsync(id);
            return Ok(r);
        }

        [HttpPost("parents/{id}/reset-password")]
        public async Task<IActionResult> ResetParentPassword(int id)
        {
            var r = await _parent.ResetPasswordAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        /// <summary>POST /api/admin/parents/{id}/photo — Upload parent profile photo</summary>
        [HttpPost("parents/{id}/photo")]
        [RequestSizeLimit(5_242_880)]
        public async Task<IActionResult> UploadParentPhoto(int id, IFormFile file)
            => await UploadUserPhoto(id, "parent", file);

        /// <summary>DELETE /api/admin/parents/{id}/photo</summary>
        [HttpDelete("parents/{id}/photo")]
        public async Task<IActionResult> DeleteParentPhoto(int id)
            => await DeleteUserPhoto(id);

        // ════════════════════════════════════════════════════════════
        // STUDENTS
        // ════════════════════════════════════════════════════════════

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents([FromQuery] int page = 1,
            [FromQuery] string? search = null, [FromQuery] string? status = null)
        {
            var r = await _student.GetAllAsync(page, 20, search, status);
            return Ok(r);
        }

        [HttpGet("students/{id}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            var r = await _student.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        [HttpPost("students")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto dto)
        {
            var r = await _student.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPut("students/{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto dto)
        {
            var r = await _student.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("students/{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var r = await _student.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("students/{id}/toggle")]
        public async Task<IActionResult> ToggleStudent(int id)
        {
            var r = await _student.ToggleActiveAsync(id);
            return Ok(r);
        }

        [HttpPost("students/{id}/reset-password")]
        public async Task<IActionResult> ResetStudentPassword(int id)
        {
            var r = await _student.ResetPasswordAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpGet("students/{id}/availability")]
        public async Task<IActionResult> GetStudentAvailability(int id)
        {
            var r = await _student.GetAvailabilitiesAsync(id);
            return Ok(r);
        }

        [HttpPost("students/{id}/availability")]
        public async Task<IActionResult> SetStudentAvailability(int id, [FromBody] CreateAvailabilityDto dto)
        {
            dto.StudentId = id;
            var r = await _student.SetAvailabilityAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpGet("students/search")]
        public async Task<IActionResult> SearchStudents([FromQuery] string? query)
        {
            var r = await _student.SearchAsync(query);
            return Ok(r);
        }

        /// <summary>POST /api/admin/students/{id}/photo — Upload student profile photo</summary>
        [HttpPost("students/{id}/photo")]
        [RequestSizeLimit(5_242_880)]
        public async Task<IActionResult> UploadStudentPhoto(int id, IFormFile file)
            => await UploadUserPhoto(id, "student", file);

        /// <summary>DELETE /api/admin/students/{id}/photo</summary>
        [HttpDelete("students/{id}/photo")]
        public async Task<IActionResult> DeleteStudentPhoto(int id)
            => await DeleteUserPhoto(id);

        // ════════════════════════════════════════════════════════════
        // TRIPS
        // ════════════════════════════════════════════════════════════

        [HttpGet("trips")]
        public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] string? busId = null)
        {
            var r = await _trip.GetAllAsync(page, 20, busId);
            return Ok(r);
        }

        [HttpGet("trips/{id}")]
        public async Task<IActionResult> GetTrip(int id)
        {
            var r = await _trip.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        [HttpPost("trips")]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDto dto)
        {
            var r = await _trip.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("trips/{id}/start")]
        public async Task<IActionResult> StartTrip(int id)
        {
            var r = await _trip.StartTripAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("trips/{id}/end")]
        public async Task<IActionResult> EndTrip(int id)
        {
            var r = await _trip.EndTripAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("trips/{id}/cancel")]
        public async Task<IActionResult> CancelTrip(int id)
        {
            var r = await _trip.CancelTripAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("trips/{id}")]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            var r = await _trip.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpGet("trips/{id}/students")]
        public async Task<IActionResult> TripStudents(int id)
        {
            var r = await _trip.GetTripStudentsAsync(id);
            return Ok(r);
        }

        [HttpGet("trips/{id}/stops")]
        public async Task<IActionResult> TripStops(int id)
        {
            var r = await _trip.GetStopEventsAsync(id);
            return Ok(r);
        }

        [HttpPost("trips/{id}/stops/{stopId}/reach")]
        public async Task<IActionResult> ReachStop(int id, int stopId)
        {
            var r = await _trip.ReachStopAsync(id, stopId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPut("trips/{id}/boarding")]
        public async Task<IActionResult> UpdateBoarding(int id, [FromBody] UpdateBoardingRequestApi req)
        {
            var r = await _trip.UpdateBoardingAsync(id, req.StudentId, req.StopId, req.Status);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpGet("trips/{id}/location")]
        public async Task<IActionResult> TripLocation(int id)
        {
            var r = await _trip.GetLatestLocationAsync(id);
            return r.Data is not null ? Ok(r) : NotFound(ApiResponse<object>.Fail("No location data yet."));
        }

        [HttpGet("trips/{id}/location/history")]
        public async Task<IActionResult> TripLocationHistory(int id)
        {
            var r = await _trip.GetLocationHistoryAsync(id);
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // FEEDBACK
        // ════════════════════════════════════════════════════════════

        [HttpGet("feedback")]
        public async Task<IActionResult> GetFeedback([FromQuery] int page = 1, [FromQuery] string? status = null)
        {
            var r = await _feedback.GetAllAsync(page, 20, status);
            return Ok(r);
        }

        [HttpPut("feedback/{id}/status")]
        public async Task<IActionResult> UpdateFeedbackStatus(int id, [FromBody] UpdateFeedbackStatusRequest req)
        {
            var r = await _feedback.UpdateStatusAsync(id, req.Status, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        // ════════════════════════════════════════════════════════════
        // NOTIFICATIONS
        // ════════════════════════════════════════════════════════════

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var r = await _notif.GetUserNotificationsAsync(CurrentUserId);
            return Ok(r);
        }

        [HttpPost("notifications/send")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest req)
        {
            await _notif.SendAsync(req.RecipientUserId, req.Title, req.Body, req.Type);
            return Ok(ApiResponse<bool>.Ok(true, "Notification sent."));
        }

        [HttpPut("notifications/{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var r = await _notif.MarkAsReadAsync(id, CurrentUserId);
            return Ok(r);
        }

        [HttpPut("notifications/read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var r = await _notif.MarkAllAsReadAsync(CurrentUserId);
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // APP CONFIGURATION
        // ════════════════════════════════════════════════════════════

        [HttpGet("config")]
        public async Task<IActionResult> GetConfigs([FromQuery] string? platform,
            [FromQuery] string? search, [FromQuery] bool? isActive, [FromQuery] int page = 1)
        {
            var r = await _config.GetAllAsync(platform, search, isActive, page);
            return Ok(r);
        }

        [HttpGet("config/{id}")]
        public async Task<IActionResult> GetConfig(int id)
        {
            var r = await _config.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        [HttpPost("config")]
        public async Task<IActionResult> CreateConfig([FromBody] CreateAppConfigDto dto)
        {
            var r = await _config.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPut("config/{id}")]
        public async Task<IActionResult> UpdateConfig(int id, [FromBody] UpdateAppConfigDto dto)
        {
            var r = await _config.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpDelete("config/{id}")]
        public async Task<IActionResult> DeleteConfig(int id)
        {
            var r = await _config.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        [HttpPost("config/{id}/toggle")]
        public async Task<IActionResult> ToggleConfig(int id)
        {
            var r = await _config.ToggleActiveAsync(id);
            return Ok(r);
        }

        // ════════════════════════════════════════════════════════════
        // SHARED IMAGE HELPERS
        // ════════════════════════════════════════════════════════════

        private async Task<IActionResult> UploadUserPhoto(int userId, string folder, IFormFile file)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return NotFound(ApiResponse<string>.Fail("User not found."));
            try
            {
                var url = await _img.SaveProfileImageAsync(file, userId, folder, user.ProfileImageUrl);
                user.ProfileImageUrl = url;
                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return Ok(ApiResponse<string>.Ok(url, "Photo updated."));
            }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<string>.Fail(ex.Message)); }
        }

        private async Task<IActionResult> DeleteUserPhoto(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return NotFound(ApiResponse<bool>.Fail("User not found."));
            _img.DeleteFile(user.ProfileImageUrl);
            user.ProfileImageUrl = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Photo removed."));
        }

        private static int ExtractIndex(string url)
        {
            try { var p = Path.GetFileNameWithoutExtension(url).Split('_'); return int.TryParse(p[^1], out var n) ? n : 0; }
            catch { return 0; }
        }
    }
}
