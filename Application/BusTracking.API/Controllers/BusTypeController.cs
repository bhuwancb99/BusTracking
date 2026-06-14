namespace BusTracking.API.Controllers
{
    /// <summary>
    /// BusType CRUD — accessible by SuperAdmin and BusCoordinator.
    /// Route: /api/bustype
    /// </summary>
    [Authorize(Roles = "SuperAdmin,BusCoordinator"), Route("api/bustype")]
    public class BusTypeController : ApiBaseController
    {
        private readonly IBusTypeService _busType;
        public BusTypeController(IBusTypeService busType) => _busType = busType;

        // GET /api/bustype
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var r = await _busType.GetAllAsync();
            return Ok(r);
        }

        // GET /api/bustype/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _busType.GetByIdAsync(id);
            return r.Success ? Ok(r) : NotFound(r);
        }

        // POST /api/bustype
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveBusTypeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<bool>.Fail("Name is required."));
            var r = await _busType.CreateAsync(dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        // PUT /api/bustype/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SaveBusTypeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<bool>.Fail("Name is required."));
            var r = await _busType.UpdateAsync(id, dto);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        // DELETE /api/bustype/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _busType.DeleteAsync(id);
            return r.Success ? Ok(r) : BadRequest(r);
        }

        // GET /api/bustype/dropdown
        [HttpGet("dropdown")]
        public async Task<IActionResult> Dropdown()
        {
            var r = await _busType.GetDropdownAsync();
            return Ok(r);
        }
    }
}
