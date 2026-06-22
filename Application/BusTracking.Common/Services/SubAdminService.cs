namespace BusTracking.Common.Services
{
    public class SubAdminService : ISubAdminService
    {
        private readonly AppDbContext _db; private readonly IPasswordService _pwd; private readonly IEmailService _email;
        public SubAdminService(AppDbContext db, IPasswordService pwd, IEmailService email) { _db = db; _pwd = pwd; _email = email; }

        public async Task<ApiResponse<PagedResult<SubAdminListDto>>> GetAllAsync(int page, string? search, string? status)
        {
            var roleId = await _db.Roles.Where(r => r.RoleName == "BusCoordinator").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users.Include(u => u.SubAdminPermissions).ThenInclude(sp => sp.Permission).Where(u => u.RoleId == roleId);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(u => u.FullName.Contains(search) || u.UserName.Contains(search) || (u.Email != null && u.Email.Contains(search)));
            if (status == "Active") q = q.Where(u => u.IsActive);
            else if (status == "Inactive") q = q.Where(u => !u.IsActive);

            var pageSize = await GetListPageSizeAsync();
            page = PaginationHelper.Clamp(page);

            var total = await q.CountAsync();
            var items = await q.OrderBy(u => u.FullName).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(u => new SubAdminListDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    ProfileImageUrl = u.ProfileImageUrl,
                    Permissions = u.SubAdminPermissions.Select(sp => sp.Permission.PermissionKey).ToList(),
                    CreatedAt = u.CreatedAt
                }).ToListAsync();
            return ApiResponse<PagedResult<SubAdminListDto>>.Ok(new PagedResult<SubAdminListDto> { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }

        public async Task<int> GetListPageSizeAsync()
        {
            var raw = await _db.AppConfigurations
                .Where(c => c.ConfigKey == AppConstants.AppConfigPageSizeKey && c.IsActive)
                .Select(c => c.ConfigValue)
                .FirstOrDefaultAsync();
            return int.TryParse(raw, out var size) && size > 0
                ? PaginationHelper.ClampPageSize(size)
                : AppConstants.DefaultPageSize;
        }

        public async Task<ApiResponse<SubAdminListDto>> GetByIdAsync(int userId)
        {
            var u = await _db.Users.Include(x => x.SubAdminPermissions).ThenInclude(sp => sp.Permission).FirstOrDefaultAsync(x => x.UserId == userId);
            if (u is null) return ApiResponse<SubAdminListDto>.Fail("Not found.");
            return ApiResponse<SubAdminListDto>.Ok(new SubAdminListDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                ProfileImageUrl = u.ProfileImageUrl,
                Permissions = u.SubAdminPermissions.Select(sp => sp.Permission.PermissionKey).ToList(),
                CreatedAt = u.CreatedAt
            });
        }

        public async Task<ApiResponse<CreatedUserResultDto>> CreateAsync(CreateSubAdminDto dto, int createdBy)
        {
            if (await _db.Users.AnyAsync(u => u.UserName == dto.UserName))
                return ApiResponse<CreatedUserResultDto>.Fail("Username already in use.");
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return ApiResponse<CreatedUserResultDto>.Fail("Email already in use.");

            var roleId = await _db.Roles.Where(r => r.RoleName == "BusCoordinator").Select(r => r.RoleId).FirstAsync();
            var password = !string.IsNullOrWhiteSpace(dto.Password) ? dto.Password : _pwd.GenerateRandomPassword();
            var (hash, salt) = _pwd.HashPassword(password);
            var user = new User
            {
                RoleId = roleId,
                FullName = dto.FullName,
                UserName = dto.UserName,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedBy = createdBy
            };
            _db.Users.Add(user); await _db.SaveChangesAsync();
            foreach (var pid in dto.PermissionIds.Distinct())
                _db.SubAdminPermissions.Add(new SubAdminPermission { UserId = user.UserId, PermissionId = pid, AssignedBy = createdBy });
            await _db.SaveChangesAsync();
            if (dto.SendEmail && !string.IsNullOrWhiteSpace(dto.Email))
                await _email.SendAsync(dto.Email!, "Your Bus Coordinator Account",
                    $"<p>Hi {dto.FullName},</p><p>Username: <b>{dto.UserName}</b><br/>Password: <b>{password}</b></p>");
            return ApiResponse<CreatedUserResultDto>.Ok(new CreatedUserResultDto
            {
                UserId = user.UserId, FullName = dto.FullName, UserName = dto.UserName,
                Email = dto.Email, PlainPassword = password, Role = "BusCoordinator"
            });
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateSubAdminDto dto)
        {
            var u = await _db.Users.FindAsync(userId); if (u is null) return ApiResponse<bool>.Fail("Not found.");
            // Check username uniqueness (excluding self)
            if (await _db.Users.AnyAsync(x => x.UserName == dto.UserName && x.UserId != userId))
                return ApiResponse<bool>.Fail("Username already in use.");
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.Users.AnyAsync(x => x.Email == dto.Email && x.UserId != userId))
                return ApiResponse<bool>.Fail("Email already in use.");

            u.FullName = dto.FullName;
            u.UserName = dto.UserName;
            u.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email;
            u.PhoneNumber = dto.PhoneNumber;
            u.IsActive = dto.IsActive;
            u.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var (hash, salt) = _pwd.HashPassword(dto.NewPassword);
                u.PasswordHash = hash; u.PasswordSalt = salt;
            }
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
            u.PasswordHash = hash; u.PasswordSalt = salt; u.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<CreatedUserResultDto>.Ok(new CreatedUserResultDto
            {
                UserId = u.UserId, FullName = u.FullName, UserName = u.UserName,
                Email = u.Email, PlainPassword = newPassword, Role = "BusCoordinator"
            }, "Password reset successfully.");
        }

        public async Task<List<int>> GetPermissionIdsAsync(int userId)
        {
            return await _db.SubAdminPermissions.Where(sp => sp.UserId == userId).Select(sp => sp.PermissionId).ToListAsync();
        }

        public async Task<List<(int Id, string ModuleName, string Key, string Description)>> GetAllPermissionsAsync()
        {
            return await _db.Permissions.OrderBy(p => p.ModuleName).ThenBy(p => p.PermissionId)
                .Select(p => ValueTuple.Create(p.PermissionId, p.ModuleName, p.PermissionKey, p.Description))
                .ToListAsync();
        }
    }
}
