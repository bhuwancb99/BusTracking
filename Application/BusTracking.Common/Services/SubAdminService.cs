using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.SubAdmin;
using BusTracking.Common.Entities;
using BusTracking.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTracking.Common.Services
{
    public class SubAdminService : ISubAdminService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordService _pwd;
        private readonly IEmailService _email;

        public SubAdminService(AppDbContext db, IPasswordService pwd, IEmailService email)
        { _db = db; _pwd = pwd; _email = email; }

        public async Task<ApiResponse<PagedResult<SubAdminListDto>>> GetAllAsync(int page, int pageSize, string? search)
        {
            var roleId = await _db.Roles.Where(r => r.RoleName == "BusCoordinator").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users
                .Include(u => u.SubAdminPermissions).ThenInclude(sp => sp.Permission)
                .Where(u => u.RoleId == roleId);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

            var total = await q.CountAsync();
            var items = await q.OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(u => new SubAdminListDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    Permissions = u.SubAdminPermissions.Select(sp => sp.Permission.PermissionKey).ToList(),
                    CreatedAt = u.CreatedAt
                }).ToListAsync();

            return ApiResponse<PagedResult<SubAdminListDto>>.Ok(new PagedResult<SubAdminListDto>
            { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }

        public async Task<ApiResponse<SubAdminListDto>> GetByIdAsync(int userId)
        {
            var u = await _db.Users
                .Include(x => x.SubAdminPermissions).ThenInclude(sp => sp.Permission)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            if (u is null) return ApiResponse<SubAdminListDto>.Fail("Coordinator not found.");

            return ApiResponse<SubAdminListDto>.Ok(new SubAdminListDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                Permissions = u.SubAdminPermissions.Select(sp => sp.Permission.PermissionKey).ToList(),
                CreatedAt = u.CreatedAt
            });
        }

        public async Task<ApiResponse<bool>> CreateAsync(CreateSubAdminDto dto, int createdBy)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return ApiResponse<bool>.Fail("Email already in use.");

            var roleId = await _db.Roles.Where(r => r.RoleName == "BusCoordinator").Select(r => r.RoleId).FirstAsync();
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

            // Assign permissions
            foreach (var pid in dto.PermissionIds.Distinct())
                _db.SubAdminPermissions.Add(new SubAdminPermission
                { UserId = user.UserId, PermissionId = pid, AssignedBy = createdBy });

            await _db.SaveChangesAsync();

            await _email.SendAsync(dto.Email, "Your Bus Coordinator Account",
                $"<p>Hi {dto.FullName},</p><p>Your Bus Coordinator account has been created.<br/>Email: {dto.Email}<br/>Password: <b>{password}</b></p><p>Please change your password after first login.</p>");

            return ApiResponse<bool>.Ok(true, "Bus Coordinator created successfully.");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateSubAdminDto dto)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return ApiResponse<bool>.Fail("Coordinator not found.");

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;

            // Replace permissions
            var existing = _db.SubAdminPermissions.Where(sp => sp.UserId == userId);
            _db.SubAdminPermissions.RemoveRange(existing);
            foreach (var pid in dto.PermissionIds.Distinct())
                _db.SubAdminPermissions.Add(new SubAdminPermission
                { UserId = userId, PermissionId = pid, AssignedBy = userId });

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Coordinator updated.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return ApiResponse<bool>.Fail("Coordinator not found.");
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Coordinator deleted.");
        }

        public async Task<ApiResponse<bool>> ToggleActiveAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return ApiResponse<bool>.Fail("Coordinator not found.");
            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, user.IsActive ? "Activated." : "Deactivated.");
        }
    }
}
