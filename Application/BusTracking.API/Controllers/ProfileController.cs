namespace BusTracking.API.Controllers
{
    [Authorize, Route("api/[controller]")]
    public class ProfileController : ApiBaseController
    {
        private readonly IUserService _user;
        public ProfileController(IUserService user) => _user = user;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var r = await _user.GetProfileAsync(CurrentUserId);
            return r.Success ? Ok(r) : NotFound(r);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
        {
            var r = await _user.UpdateProfileAsync(CurrentUserId, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }
    }
}
