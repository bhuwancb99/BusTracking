namespace BusTracking.Common.Services
{
    public class SubAdminService : ISubAdminService
    {
        private readonly AppDbContext _db; private readonly IPasswordService _pwd; private readonly IEmailService _email;
        public SubAdminService(AppDbContext db, IPasswordService pwd, IEmailService email) { _db = db; _pwd = pwd; _email = email; }

        public async Task<ApiResponse<PagedResult<SubAdminListDto>>> GetAllAsync(int page, int pageSize, string? search, string? status)
        {
            var roleId = await _db.Roles.Where(r => r.RoleName == "BusCoordinator").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users.Include(u => u.SubAdminPermissions).ThenInclude(sp => sp.Permission).Where(u => u.RoleId == roleId);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
            if (status == "Active") q = q.Where(u => u.IsActive);
            else if (status == "Inactive") q = q.Where(u => !u.IsActive);
            var total = await q.CountAsync();
            var items = await q.OrderBy(u => u.FullName).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(u => new SubAdminListDto { UserId = u.UserId, FullName = u.FullName, Email = u.Email, PhoneNumber = u.PhoneNumber, IsActive = u.IsActive, Permissions = u.SubAdminPermissions.Select(sp => sp.Permission.PermissionKey).ToList(), CreatedAt = u.CreatedAt }).ToListAsync();
            return ApiResponse<PagedResult<SubAdminListDto>>.Ok(new PagedResult<SubAdminListDto> { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }
        public async Task<ApiResponse<SubAdminListDto>> GetByIdAsync(int userId)
        {
            var u = await _db.Users.Include(x => x.SubAdminPermissions).ThenInclude(sp => sp.Permission).FirstOrDefaultAsync(x => x.UserId == userId);
            if (u is null) return ApiResponse<SubAdminListDto>.Fail("Not found.");
            return ApiResponse<SubAdminListDto>.Ok(new SubAdminListDto { UserId = u.UserId, FullName = u.FullName, Email = u.Email, PhoneNumber = u.PhoneNumber, IsActive = u.IsActive, Permissions = u.SubAdminPermissions.Select(sp => sp.Permission.PermissionKey).ToList(), CreatedAt = u.CreatedAt });
        }
        public async Task<ApiResponse<CreatedUserResultDto>> CreateAsync(CreateSubAdminDto dto, int createdBy)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email)) return ApiResponse<CreatedUserResultDto>.Fail("Email already in use.");
            var roleId = await _db.Roles.Where(r => r.RoleName == "BusCoordinator").Select(r => r.RoleId).FirstAsync();
            var password = _pwd.GenerateRandomPassword();
            var (hash, salt) = _pwd.HashPassword(password);
            var user = new User { RoleId = roleId, FullName = dto.FullName, Email = dto.Email, PhoneNumber = dto.PhoneNumber, PasswordHash = hash, PasswordSalt = salt, CreatedBy = createdBy };
            _db.Users.Add(user); await _db.SaveChangesAsync();
            foreach (var pid in dto.PermissionIds.Distinct())
                _db.SubAdminPermissions.Add(new SubAdminPermission { UserId = user.UserId, PermissionId = pid, AssignedBy = createdBy });
            await _db.SaveChangesAsync();
            if (dto.SendEmail) await _email.SendAsync(dto.Email, "Your Bus Coordinator Account", $"<p>Hi {dto.FullName},</p><p>Email: {dto.Email}<br/>Password: <b>{password}</b></p>");
            return ApiResponse<CreatedUserResultDto>.Ok(new CreatedUserResultDto { UserId = user.UserId, FullName = dto.FullName, Email = dto.Email, PlainPassword = password, Role = "BusCoordinator" });
        }
        public async Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateSubAdminDto dto)
        {
            var u = await _db.Users.FindAsync(userId); if (u is null) return ApiResponse<bool>.Fail("Not found.");
            u.FullName = dto.FullName; u.PhoneNumber = dto.PhoneNumber; u.IsActive = dto.IsActive; u.UpdatedAt = DateTime.UtcNow;
            var existing = _db.SubAdminPermissions.Where(sp => sp.UserId == userId);
            _db.SubAdminPermissions.RemoveRange(existing);
            foreach (var pid in dto.PermissionIds.Distinct())
                _db.SubAdminPermissions.Add(new SubAdminPermission { UserId = userId, PermissionId = pid, AssignedBy = userId });
            await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Updated.");
        }
        public async Task<ApiResponse<bool>> DeleteAsync(int userId)
        {
            var u = await _db.Users.FindAsync(userId); if (u is null) return ApiResponse<bool>.Fail("Not found.");
            u.IsActive = false; u.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Marked inactive.");
        }
        public async Task<ApiResponse<bool>> ToggleActiveAsync(int userId)
        {
            var u = await _db.Users.FindAsync(userId); if (u is null) return ApiResponse<bool>.Fail("Not found.");
            u.IsActive = !u.IsActive; u.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, u.IsActive ? "Activated." : "Deactivated.");
        }
        public async Task<ApiResponse<CreatedUserResultDto>> ResetPasswordAsync(int userId)
        {
            var u = await _db.Users.FindAsync(userId);
            if (u is null) return ApiResponse<CreatedUserResultDto>.Fail("Coordinator not found.");
            var newPassword = _pwd.GenerateRandomPassword();
            var (hash, salt) = _pwd.HashPassword(newPassword);
            u.PasswordHash = hash;
            u.PasswordSalt = salt;
            u.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<CreatedUserResultDto>.Ok(new CreatedUserResultDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PlainPassword = newPassword,
                Role = "BusCoordinator"
            }, "Password reset successfully.");
        }

        public async Task<List<int>> GetPermissionIdsAsync(int userId)
        {
            return await _db.SubAdminPermissions
                .Where(sp => sp.UserId == userId)
                .Select(sp => sp.PermissionId)
                .ToListAsync();
        }

        public async Task<List<(int Id, string ModuleName, string Key, string Description)>> GetAllPermissionsAsync()
        {
            return await _db.Permissions
                .OrderBy(p => p.ModuleName).ThenBy(p => p.PermissionId)
                .Select(p => ValueTuple.Create(p.PermissionId, p.ModuleName, p.PermissionKey, p.Description))
                .ToListAsync();
        }
    }
}
