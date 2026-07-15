namespace BusTracking.API.Controllers
{
    [Authorize, Route("api/standards")]
    public class StandardsController : ApiBaseController
    {
        private readonly IStudentService _student;
        public StandardsController(IStudentService student) => _student = student;

        [HttpGet]
        public async Task<IActionResult> GetStandards()
        {
            var r = await _student.GetStandardsAsync();
            return r.Success ? Ok(r) : BadRequest(r);
        }
    }
}
