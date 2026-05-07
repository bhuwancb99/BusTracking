using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Driver;
using BusTracking.Common.Entities;
using BusTracking.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTracking.Common.Services
{
    public class DriverService : IDriverService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordService _pwd;
        private readonly IEmailService _email;

        public DriverService(AppDbContext db, IPasswordService pwd, IEmailService email)
        { _db = db; _pwd = pwd; _email = email; }

        public async Task<ApiResponse<PagedResult<DriverListDto>>> GetAllAsync(int page, int pageSize, string? search)
        {
            var roleId = await _db.Roles.Where(r => r.RoleName == "Driver").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users
                .Include(u => u.DriverDetail).ThenInclude(d => d!.Bus)
                .Where(u => u.RoleId == roleId);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            var total = await q.CountAsync();
            var items = await q.OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(u => new DriverListDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    BusNumber = u.DriverDetail != null && u.DriverDetail.Bus != null ? u.DriverDetail.Bus.BusNumber : null,
                    BusName = u.DriverDetail != null && u.DriverDetail.Bus != null ? u.DriverDetail.Bus.BusName : null,
                    LicenseNumber = u.DriverDetail != null ? u.DriverDetail.LicenseNumber : null,
                    IsActive = u.IsActive
                }).ToListAsync();

            return ApiResponse<PagedResult<DriverListDto>>.Ok(new PagedResult<DriverListDto>
            { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }

        public async Task<ApiResponse<DriverListDto>> GetByIdAsync(int userId)
        {
            var u = await _db.Users
                .Include(x => x.DriverDetail).ThenInclude(d => d!.Bus)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            if (u is null) return ApiResponse<DriverListDto>.Fail("Driver not found.");

            return ApiResponse<DriverListDto>.Ok(new DriverListDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                BusNumber = u.DriverDetail?.Bus?.BusNumber,
                BusName = u.DriverDetail?.Bus?.BusName,
                LicenseNumber = u.DriverDetail?.LicenseNumber,
                IsActive = u.IsActive
            });
        }

        public async Task<ApiResponse<bool>> CreateAsync(CreateDriverDto dto, int createdBy)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return ApiResponse<bool>.Fail("Email already in use.");

            var roleId = await _db.Roles.Where(r => r.RoleName == "Driver").Select(r => r.RoleId).FirstAsync();
            var password = _pwd.GenerateRandomPassword();
            var (hash, salt) = _pwd.HashPassword(password);

            var user = new User
            {
                RoleId = roleId,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedBy = createdBy
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.DriverDetails.Add(new DriverDetail
            {
                UserId = user.UserId,
                LicenseNumber = dto.LicenseNumber,
                LicenseExpiry = dto.LicenseExpiry is not null ? DateOnly.Parse(dto.LicenseExpiry) : null,
                BusId = dto.BusId
            });
            await _db.SaveChangesAsync();

            await _email.SendAsync(dto.Email, "Your Driver Account - Bus Tracking",
                $"<p>Hi {dto.FullName},</p><p>Your driver account has been created.<br/>Email: {dto.Email}<br/>Password: <b>{password}</b></p><p>Use the mobile app to start tracking.</p>");

            return ApiResponse<bool>.Ok(true, "Driver created.");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateDriverDto dto)
        {
            var user = await _db.Users.Include(u => u.DriverDetail).FirstOrDefaultAsync(u => u.UserId == userId);
            if (user is null) return ApiResponse<bool>.Fail("Driver not found.");

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            if (user.DriverDetail is not null)
            {
                user.DriverDetail.LicenseNumber = dto.LicenseNumber;
                user.DriverDetail.LicenseExpiry = dto.LicenseExpiry is not null ? DateOnly.Parse(dto.LicenseExpiry) : null;
                user.DriverDetail.BusId = dto.BusId;
                user.DriverDetail.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Driver updated.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return ApiResponse<bool>.Fail("Driver not found.");
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Driver deleted.");
        }

        public async Task<ApiResponse<bool>> AssignBusAsync(int driverUserId, int busId)
        {
            var detail = await _db.DriverDetails.FirstOrDefaultAsync(d => d.UserId == driverUserId);
            if (detail is null) return ApiResponse<bool>.Fail("Driver not found.");
            detail.BusId = busId;
            detail.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Bus assigned to driver.");
        }
    }
}
