using BusTracking.Common.DTOs.Feedback;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTracking.API.Controllers
{
    [Authorize, Route("api/[controller]")]
    public class FeedbackController : ApiBaseController
    {
        private readonly IFeedbackService _feedback;
        public FeedbackController(IFeedbackService feedback) => _feedback = feedback;

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] CreateFeedbackDto dto)
        {
            var r = await _feedback.CreateAsync(dto, CurrentUserId);
            return r.Success ? Ok(r) : BadRequest(r);
        }
    }
}
