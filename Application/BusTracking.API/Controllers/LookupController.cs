namespace BusTracking.API.Controllers
{
    [ApiController]
    [Route("api/lookup")]
    [AllowAnonymous]
    public class LookupController : ControllerBase
    {
        private readonly IGeographicService _geoService;

        public LookupController(IGeographicService geoService)
        {
            _geoService = geoService;
        }

        [HttpGet("countries")]
        public async Task<IActionResult> GetActiveCountries()
        {
            var countries = await _geoService.GetActiveCountriesLookupAsync();
            return Ok(new { success = true, data = countries });
        }

        [HttpGet("countries/{countryId:int}/regions")]
        public async Task<IActionResult> GetActiveRegions(int countryId)
        {
            var regions = await _geoService.GetActiveRegionsLookupAsync(countryId);
            return Ok(new { success = true, data = regions });
        }
    }
}
