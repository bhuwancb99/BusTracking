namespace BusTracking.API.Controllers
{
    [Authorize(Roles = "Driver"), Route("api/trips/{tripId}/boarding")]
    public class BoardingController : ApiBaseController
    {
        private readonly AppDbContext _db;
        private readonly IFcmPushNotificationService _fcm;

        public BoardingController(AppDbContext db, IFcmPushNotificationService fcm)
        {
            _db = db;
            _fcm = fcm;
        }

        public class UpdateBoardingRequest
        {
            public int StudentId { get; set; }
            public int StopId { get; set; }
            public string BoardingStatus { get; set; } = "";
            public string Status { get; set; } = "";
        }

        public class QuickScanBoardingRequest
        {
            public string StudentCode { get; set; } = "";
            public string BoardingStatus { get; set; } = "PickedUp";
        }

        /// <summary>Driver marks student as PickedUp, NoShow, OnLeave, or Pending</summary>
        [HttpPut]
        public async Task<IActionResult> UpdateBoarding(int tripId, [FromBody] UpdateBoardingRequest req)
        {
            var rawStatus = !string.IsNullOrWhiteSpace(req.BoardingStatus) ? req.BoardingStatus : req.Status;

            if (!Enum.TryParse<BoardingStatus>(rawStatus, true, out var status))
                return BadRequest(ApiResponse<bool>.Fail($"Invalid boarding status '{rawStatus}'."));

            var now = TimeZoneHelper.GetNow(CurrentTimeZoneInfoId);

            var existing = await _db.StudentTripStatuses
                .FirstOrDefaultAsync(s => s.TripId == tripId && s.StudentId == req.StudentId);

            if (existing is null)
            {
                _db.StudentTripStatuses.Add(new StudentTripStatus
                {
                    TripId = tripId,
                    StudentId = req.StudentId,
                    StopId = req.StopId,
                    BoardingStatus = status,
                    UpdatedAt = now,
                    UpdatedBy = CurrentUserId
                });
            }
            else
            {
                existing.BoardingStatus = status;
                existing.UpdatedAt = now;
                existing.UpdatedBy = CurrentUserId;
            }

            await _db.SaveChangesAsync();

            if (status == BoardingStatus.PickedUp)
            {
                _ = _fcm.SendStudentPickedUpPushAsync(tripId, req.StudentId, req.StopId);
            }

            return Ok(ApiResponse<bool>.Ok(true, $"Status updated to {status}."));
        }

        /// <summary>Driver scans student QR / StudentCode to mark boarding status instantly</summary>
        [HttpPost("scan-code")]
        public async Task<IActionResult> QuickScanBoarding(int tripId, [FromBody] QuickScanBoardingRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.StudentCode))
                return BadRequest(ApiResponse<bool>.Fail("StudentCode is required."));

            var code = req.StudentCode.Trim();
            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentCode == code);

            if (student is null)
                return NotFound(ApiResponse<bool>.Fail($"Student code '{code}' not found."));

            var stopId = student.StopId ?? 0;
            return await UpdateBoarding(tripId, new UpdateBoardingRequest
            {
                StudentId = student.StudentId,
                StopId = stopId,
                BoardingStatus = req.BoardingStatus
            });
        }
    }
}
