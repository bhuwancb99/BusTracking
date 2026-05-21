namespace BusTracking.API.Controllers
{
    [Authorize(Roles = "Driver"), Route("api/trips/{tripId}/boarding")]
    public class BoardingController : ApiBaseController
    {
        private readonly AppDbContext _db;
        public BoardingController(AppDbContext db) => _db = db;

        public class UpdateBoardingRequest
        {
            public int StudentId { get; set; }
            public int StopId { get; set; }
            public string BoardingStatus { get; set; } = "";   // PickedUp | NoShow
        }

        /// <summary>Driver marks student as PickedUp or NoShow</summary>
        [HttpPut]
        public async Task<IActionResult> UpdateBoarding(int tripId, [FromBody] UpdateBoardingRequest req)
        {
            if (!Enum.TryParse<BoardingStatus>(req.BoardingStatus, true, out var status))
                return BadRequest(ApiResponse<bool>.Fail("Invalid boarding status."));

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
                    UpdatedBy = CurrentUserId
                });
            }
            else
            {
                existing.BoardingStatus = status;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = CurrentUserId;
            }

            await _db.SaveChangesAsync();
            return Ok(ApiResponse<bool>.Ok(true, "Status updated."));
        }
    }
}
