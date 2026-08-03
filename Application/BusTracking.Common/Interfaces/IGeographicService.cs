namespace BusTracking.Common.Interfaces
{
    public interface IGeographicService
    {
        // SystemAdmin / Admin CRUD Operations
        Task<List<CountryDto>> GetAllCountriesAsync(bool includeInactive = true);
        Task<CountryDto?> GetCountryByIdAsync(int countryId);
        Task<CountryDto> CreateCountryAsync(CreateCountryDto dto);
        Task<CountryDto> UpdateCountryAsync(int countryId, UpdateCountryDto dto);
        Task<bool> DeleteCountryAsync(int countryId);

        Task<List<RegionDto>> GetRegionsByCountryAsync(int countryId, bool includeInactive = true);
        Task<RegionDto?> GetRegionByIdAsync(int regionId);
        Task<RegionDto> CreateRegionAsync(CreateRegionDto dto);
        Task<RegionDto> UpdateRegionAsync(int regionId, UpdateRegionDto dto);
        Task<bool> DeleteRegionAsync(int regionId);

        // Public / App Dropdown Lookups (Active only)
        Task<List<CountryDto>> GetActiveCountriesLookupAsync();
        Task<List<RegionDto>> GetActiveRegionsLookupAsync(int countryId);
    }
}
