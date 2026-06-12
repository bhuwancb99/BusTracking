namespace BusTracking.Common.Services
{
    public class BusTypeService : IBusTypeService
    {
        private readonly AppDbContext _db;
        public BusTypeService(AppDbContext db) => _db = db;

        public async Task<ApiResponse<List<BusTypeDto>>> GetAllAsync()
        {
            var list = await _db.BusTypeMasters
                .OrderBy(t => t.Name)
                .Select(t => new BusTypeDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    BusCount = t.Buses.Count,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToListAsync();

            return ApiResponse<List<BusTypeDto>>.Ok(list);
        }

        public async Task<ApiResponse<BusTypeDto>> GetByIdAsync(int id)
        {
            var t = await _db.BusTypeMasters.FindAsync(id);
            if (t is null)
                return ApiResponse<BusTypeDto>.Fail("Bus type not found.");

            return ApiResponse<BusTypeDto>.Ok(new BusTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                BusCount = await _db.Buses.CountAsync(b => b.BusTypeId == t.Id),
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            });
        }

        public async Task<ApiResponse<bool>> CreateAsync(SaveBusTypeDto dto)
        {
            var name = dto.Name.Trim();
            if (await _db.BusTypeMasters.AnyAsync(t => t.Name == name))
                return ApiResponse<bool>.Fail($"Bus type '{name}' already exists.");

            _db.BusTypeMasters.Add(new BusTypeMaster
            {
                Name = name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Bus type added successfully.");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, SaveBusTypeDto dto)
        {
            var t = await _db.BusTypeMasters.FindAsync(id);
            if (t is null)
                return ApiResponse<bool>.Fail("Bus type not found.");

            var name = dto.Name.Trim();
            if (await _db.BusTypeMasters.AnyAsync(x => x.Name == name && x.Id != id))
                return ApiResponse<bool>.Fail($"Bus type '{name}' already exists.");

            t.Name = name;
            t.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Bus type updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            var t = await _db.BusTypeMasters.FindAsync(id);
            if (t is null)
                return ApiResponse<bool>.Fail("Bus type not found.");

            var inUse = await _db.Buses.AnyAsync(b => b.BusTypeId == id);
            if (inUse)
                return ApiResponse<bool>.Fail("Cannot delete — this bus type is assigned to one or more buses.");

            _db.BusTypeMasters.Remove(t);
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Bus type deleted.");
        }

        public async Task<ApiResponse<List<BusTypeDropdownDto>>> GetDropdownAsync()
        {
            var list = await _db.BusTypeMasters
                .OrderBy(t => t.Name)
                .Select(t => new BusTypeDropdownDto { Id = t.Id, Name = t.Name })
                .ToListAsync();

            return ApiResponse<List<BusTypeDropdownDto>>.Ok(list);
        }
    }
}
