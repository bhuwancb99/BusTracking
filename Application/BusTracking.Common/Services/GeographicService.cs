namespace BusTracking.Common.Services
{
    public class GeographicService : IGeographicService
    {
        private readonly AppDbContext _db;

        public GeographicService(AppDbContext db)
        {
            _db = db;
        }

        #region Country Operations

        public async Task<List<CountryDto>> GetAllCountriesAsync(bool includeInactive = true)
        {
            var query = _db.CountryMasters.AsNoTracking();

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            return await query
                .OrderBy(c => c.CountryName)
                .Select(c => new CountryDto
                {
                    CountryId = c.CountryId,
                    CountryName = c.CountryName,
                    ISO2 = c.ISO2,
                    PhoneCode = c.PhoneCode,
                    CurrencyCode = c.CurrencyCode,
                    CurrencySymbol = c.CurrencySymbol,
                    IsActive = c.IsActive,
                    ActiveRegionCount = c.Regions.Count(r => r.IsActive),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<CountryDto?> GetCountryByIdAsync(int countryId)
        {
            return await _db.CountryMasters.AsNoTracking()
                .Where(c => c.CountryId == countryId)
                .Select(c => new CountryDto
                {
                    CountryId = c.CountryId,
                    CountryName = c.CountryName,
                    ISO2 = c.ISO2,
                    PhoneCode = c.PhoneCode,
                    CurrencyCode = c.CurrencyCode,
                    CurrencySymbol = c.CurrencySymbol,
                    IsActive = c.IsActive,
                    ActiveRegionCount = c.Regions.Count(r => r.IsActive),
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<CountryDto> CreateCountryAsync(CreateCountryDto dto)
        {
            if (await _db.CountryMasters.AnyAsync(c => c.CountryName.ToLower() == dto.CountryName.ToLower()))
                throw new InvalidOperationException($"Country '{dto.CountryName}' already exists.");

            var country = new CountryMaster
            {
                CountryName = dto.CountryName.Trim(),
                ISO2 = dto.ISO2?.Trim().ToUpper(),
                PhoneCode = dto.PhoneCode?.Trim(),
                CurrencyCode = dto.CurrencyCode?.Trim().ToUpper(),
                CurrencySymbol = dto.CurrencySymbol?.Trim(),
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.CountryMasters.Add(country);
            await _db.SaveChangesAsync();

            return (await GetCountryByIdAsync(country.CountryId))!;
        }

        public async Task<CountryDto> UpdateCountryAsync(int countryId, UpdateCountryDto dto)
        {
            var country = await _db.CountryMasters.FindAsync(countryId)
                ?? throw new KeyNotFoundException($"Country with ID {countryId} not found.");

            if (await _db.CountryMasters.AnyAsync(c => c.CountryId != countryId && c.CountryName.ToLower() == dto.CountryName.ToLower()))
                throw new InvalidOperationException($"Country '{dto.CountryName}' already exists.");

            country.CountryName = dto.CountryName.Trim();
            country.ISO2 = dto.ISO2?.Trim().ToUpper();
            country.PhoneCode = dto.PhoneCode?.Trim();
            country.CurrencyCode = dto.CurrencyCode?.Trim().ToUpper();
            country.CurrencySymbol = dto.CurrencySymbol?.Trim();
            country.IsActive = dto.IsActive;
            country.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return (await GetCountryByIdAsync(country.CountryId))!;
        }

        public async Task<bool> DeleteCountryAsync(int countryId)
        {
            var country = await _db.CountryMasters.FindAsync(countryId);
            if (country == null) return false;

            _db.CountryMasters.Remove(country);
            await _db.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Region Operations

        public async Task<List<RegionDto>> GetRegionsByCountryAsync(int countryId, bool includeInactive = true)
        {
            var query = _db.RegionMasters.AsNoTracking()
                .Include(r => r.Country)
                .Where(r => r.CountryId == countryId);

            if (!includeInactive)
                query = query.Where(r => r.IsActive);

            return await query
                .OrderBy(r => r.RegionName)
                .Select(r => new RegionDto
                {
                    RegionId = r.RegionId,
                    CountryId = r.CountryId,
                    CountryName = r.Country.CountryName,
                    RegionName = r.RegionName,
                    RegionCode = r.RegionCode,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<RegionDto?> GetRegionByIdAsync(int regionId)
        {
            return await _db.RegionMasters.AsNoTracking()
                .Include(r => r.Country)
                .Where(r => r.RegionId == regionId)
                .Select(r => new RegionDto
                {
                    RegionId = r.RegionId,
                    CountryId = r.CountryId,
                    CountryName = r.Country.CountryName,
                    RegionName = r.RegionName,
                    RegionCode = r.RegionCode,
                    IsActive = r.IsActive,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<RegionDto> CreateRegionAsync(CreateRegionDto dto)
        {
            if (!await _db.CountryMasters.AnyAsync(c => c.CountryId == dto.CountryId))
                throw new KeyNotFoundException($"Country with ID {dto.CountryId} not found.");

            if (await _db.RegionMasters.AnyAsync(r => r.CountryId == dto.CountryId && r.RegionName.ToLower() == dto.RegionName.ToLower()))
                throw new InvalidOperationException($"Region '{dto.RegionName}' already exists in this country.");

            var region = new RegionMaster
            {
                CountryId = dto.CountryId,
                RegionName = dto.RegionName.Trim(),
                RegionCode = dto.RegionCode?.Trim().ToUpper(),
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.RegionMasters.Add(region);
            await _db.SaveChangesAsync();

            return (await GetRegionByIdAsync(region.RegionId))!;
        }

        public async Task<RegionDto> UpdateRegionAsync(int regionId, UpdateRegionDto dto)
        {
            var region = await _db.RegionMasters.FindAsync(regionId)
                ?? throw new KeyNotFoundException($"Region with ID {regionId} not found.");

            if (await _db.RegionMasters.AnyAsync(r => r.RegionId != regionId && r.CountryId == region.CountryId && r.RegionName.ToLower() == dto.RegionName.ToLower()))
                throw new InvalidOperationException($"Region '{dto.RegionName}' already exists in this country.");

            region.RegionName = dto.RegionName.Trim();
            region.RegionCode = dto.RegionCode?.Trim().ToUpper();
            region.IsActive = dto.IsActive;
            region.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return (await GetRegionByIdAsync(region.RegionId))!;
        }

        public async Task<bool> DeleteRegionAsync(int regionId)
        {
            var region = await _db.RegionMasters.FindAsync(regionId);
            if (region == null) return false;

            _db.RegionMasters.Remove(region);
            await _db.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Lookups for Application Dropdowns

        public async Task<List<CountryDto>> GetActiveCountriesLookupAsync()
        {
            return await GetAllCountriesAsync(includeInactive: false);
        }

        public async Task<List<RegionDto>> GetActiveRegionsLookupAsync(int countryId)
        {
            return await GetRegionsByCountryAsync(countryId, includeInactive: false);
        }

        #endregion
    }
}
