using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Bus;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.Entities;
using BusTracking.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTracking.Common.Services
{
    public class BusService : IBusService
    {
        private readonly AppDbContext _db;
        public BusService(AppDbContext db) => _db = db;

        public async Task<ApiResponse<PagedResult<BusListDto>>> GetAllAsync(int page, int pageSize, string? search)
        {
            var q = _db.Buses
                .Include(b => b.Route)
                .Include(b => b.Driver).ThenInclude(d => d!.User)
                .Where(b => b.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(b => b.BusName.Contains(search) || b.BusNumber.Contains(search));

            var total = await q.CountAsync();
            var items = await q.OrderBy(b => b.BusName)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(b => new BusListDto
                {
                    BusId = b.BusId,
                    BusName = b.BusName,
                    BusNumber = b.BusNumber,
                    RouteName = b.Route != null ? b.Route.RouteName : null,
                    DriverName = b.Driver != null ? b.Driver.User.FullName : null,
                    DriverPhone = b.Driver != null ? b.Driver.User.PhoneNumber : null,
                    Capacity = b.Capacity,
                    IsActive = b.IsActive
                }).ToListAsync();

            return ApiResponse<PagedResult<BusListDto>>.Ok(new PagedResult<BusListDto>
            { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }

        public async Task<ApiResponse<BusDetailDto>> GetByIdAsync(int busId)
        {
            var b = await _db.Buses
                .Include(x => x.Route)
                .Include(x => x.Driver).ThenInclude(d => d!.User)
                .Include(x => x.Students)
                .FirstOrDefaultAsync(x => x.BusId == busId);

            if (b is null) return ApiResponse<BusDetailDto>.Fail("Bus not found.");

            return ApiResponse<BusDetailDto>.Ok(new BusDetailDto
            {
                BusId = b.BusId,
                BusName = b.BusName,
                BusNumber = b.BusNumber,
                RouteId = b.RouteId,
                RouteName = b.Route?.RouteName,
                RouteCode = b.Route?.RouteCode,
                DriverUserId = b.Driver?.UserId,
                DriverName = b.Driver?.User.FullName,
                DriverPhone = b.Driver?.User.PhoneNumber,
                Capacity = b.Capacity,
                StudentCount = b.Students.Count,
                IsActive = b.IsActive
            });
        }

        public async Task<ApiResponse<bool>> CreateAsync(CreateBusDto dto, int createdBy)
        {
            if (await _db.Buses.AnyAsync(b => b.BusNumber == dto.BusNumber))
                return ApiResponse<bool>.Fail("Bus number already exists.");

            _db.Buses.Add(new Bus
            {
                BusName = dto.BusName,
                BusNumber = dto.BusNumber,
                RouteId = dto.RouteId,
                Capacity = dto.Capacity,
                CreatedBy = createdBy
            });
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Bus created.");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int busId, UpdateBusDto dto)
        {
            var bus = await _db.Buses.FindAsync(busId);
            if (bus is null) return ApiResponse<bool>.Fail("Bus not found.");

            if (await _db.Buses.AnyAsync(b => b.BusNumber == dto.BusNumber && b.BusId != busId))
                return ApiResponse<bool>.Fail("Bus number already in use.");

            bus.BusName = dto.BusName;
            bus.BusNumber = dto.BusNumber;
            bus.RouteId = dto.RouteId;
            bus.Capacity = dto.Capacity;
            bus.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Bus updated.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int busId)
        {
            var bus = await _db.Buses.FindAsync(busId);
            if (bus is null) return ApiResponse<bool>.Fail("Bus not found.");
            bus.IsActive = false;
            bus.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Bus deleted.");
        }

        public async Task<ApiResponse<bool>> AssignStudentAsync(int busId, int studentId)
        {
            var student = await _db.Students.FindAsync(studentId);
            if (student is null) return ApiResponse<bool>.Fail("Student not found.");
            student.BusId = busId;
            student.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Student assigned to bus.");
        }

        public async Task<ApiResponse<bool>> RemoveStudentAsync(int busId, int studentId)
        {
            var student = await _db.Students.FindAsync(studentId);
            if (student is null) return ApiResponse<bool>.Fail("Student not found.");
            student.BusId = null;
            student.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Student removed from bus.");
        }
    }
}
