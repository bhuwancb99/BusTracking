namespace BusTracking.Common.Services
{
    public class DriverService : IDriverService
    {
        private readonly AppDbContext _db; private readonly IPasswordService _pwd; private readonly IEmailService _email;
        public DriverService(AppDbContext db, IPasswordService pwd, IEmailService email) { _db = db; _pwd = pwd; _email = email; }

        public async Task<ApiResponse<PagedResult<DriverListDto>>> GetAllAsync(int page, string? search, string? status)
        {
            var roleId = await _db.Roles.Where(r => r.RoleName == "Driver").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users.Include(u => u.DriverDetail).ThenInclude(d => d!.Bus).Where(u => u.RoleId == roleId);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
            if (status == "Active") q = q.Where(u => u.IsActive);
            else if (status == "Inactive") q = q.Where(u => !u.IsActive);

            var pageSize = await GetListPageSizeAsync();
            page = PaginationHelper.Clamp(page);

            var total = await q.CountAsync();
            var items = await q.OrderBy(u => u.FullName).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(u => new DriverListDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    BusId = u.DriverDetail != null ? u.DriverDetail.BusId : null,
                    BusName = u.DriverDetail != null && u.DriverDetail.Bus != null ? u.DriverDetail.Bus.BusName : null,
                    BusNumber = u.DriverDetail != null && u.DriverDetail.Bus != null ? u.DriverDetail.Bus.BusNumber : null,
                    LicenseNumber = u.DriverDetail != null ? u.DriverDetail.LicenseNumber : null,
                    LicenseExpiry = u.DriverDetail != null && u.DriverDetail.LicenseExpiry != null ? u.DriverDetail.LicenseExpiry.Value.ToString("yyyy-MM-dd") : null,
                    IsActive = u.IsActive
                }).ToListAsync();
            return ApiResponse<PagedResult<DriverListDto>>.Ok(new PagedResult<DriverListDto> { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
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
        public async Task<ApiResponse<DriverListDto>> GetByIdAsync(int userId)
        {
            var u = await _db.Users.Include(x => x.DriverDetail).ThenInclude(d => d!.Bus).FirstOrDefaultAsync(x => x.UserId == userId);
            if (u is null) return ApiResponse<DriverListDto>.Fail("Not found.");
            return ApiResponse<DriverListDto>.Ok(new DriverListDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                BusId = u.DriverDetail?.BusId,
                BusName = u.DriverDetail?.Bus?.BusName,
                BusNumber = u.DriverDetail?.Bus?.BusNumber,
                LicenseNumber = u.DriverDetail?.LicenseNumber,
                LicenseExpiry = u.DriverDetail?.LicenseExpiry?.ToString("yyyy-MM-dd"),
                IsActive = u.IsActive,
                ProfileImageUrl = u.ProfileImageUrl
            });
        }
        public async Task<ApiResponse<CreatedUserResultDto>> CreateAsync(CreateDriverDto dto, int createdBy)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email)) return ApiResponse<CreatedUserResultDto>.Fail("Email already in use.");
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
            _db.Users.Add(user); await _db.SaveChangesAsync();
            _db.DriverDetails.Add(new DriverDetail
            {
                UserId = user.UserId,
                LicenseNumber = dto.LicenseNumber,
                LicenseExpiry = dto.LicenseExpiry is not null ? DateOnly.Parse(dto.LicenseExpiry) : null,
                BusId = dto.BusId
            });
            await _db.SaveChangesAsync();
            if (dto.SendEmail) await _email.SendAsync(dto.Email, "Your Driver Account", $"<p>Hi {dto.FullName},</p><p>Email: {dto.Email}<br/>Password: <b>{password}</b></p>");
            return ApiResponse<CreatedUserResultDto>.Ok(new CreatedUserResultDto
            {
                UserId = user.UserId,
                FullName = dto.FullName,
                Email = dto.Email,
                PlainPassword = password,
                Role = "Driver"
            });
        }
        public async Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateDriverDto dto)
        {
            var u = await _db.Users.Include(x => x.DriverDetail).FirstOrDefaultAsync(x => x.UserId == userId);
            if (u is null) return ApiResponse<bool>.Fail("Not found.");
            u.FullName = dto.FullName;
            u.PhoneNumber = dto.PhoneNumber;
            u.IsActive = dto.IsActive;
            u.UpdatedAt = DateTime.UtcNow;
            if (u.DriverDetail is not null)
            { u.DriverDetail.LicenseNumber = dto.LicenseNumber; u.DriverDetail.LicenseExpiry = dto.LicenseExpiry is not null ? DateOnly.Parse(dto.LicenseExpiry) : null; u.DriverDetail.BusId = dto.BusId; u.DriverDetail.UpdatedAt = DateTime.UtcNow; }
            await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Updated.");
        }
        public async Task<ApiResponse<bool>> DeleteAsync(int userId)
        { var u = await _db.Users.FindAsync(userId); if (u is null) return ApiResponse<bool>.Fail("Not found."); u.IsActive = false; u.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Marked inactive."); }
        public async Task<ApiResponse<bool>> ToggleActiveAsync(int userId)
        { var u = await _db.Users.FindAsync(userId); if (u is null) return ApiResponse<bool>.Fail("Not found."); u.IsActive = !u.IsActive; u.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, u.IsActive ? "Activated." : "Deactivated."); }
        public async Task<ApiResponse<bool>> AssignBusAsync(AssignBusToDriverDto dto)
        {
            var d = await _db.DriverDetails.FirstOrDefaultAsync(x => x.UserId == dto.DriverUserId); if (d is null) return ApiResponse<bool>.Fail("Driver not found.");
            var prev = await _db.DriverDetails.FirstOrDefaultAsync(x => x.BusId == dto.BusId && x.UserId != dto.DriverUserId);
            if (prev is not null) { prev.BusId = null; prev.UpdatedAt = DateTime.UtcNow; }
            d.BusId = dto.BusId; d.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Bus assigned.");
        }
        public async Task<ApiResponse<List<DriverDropdownDto>>> GetDropdownAsync(string? search)
        {
            var roleId = await _db.Roles.Where(r => r.RoleName == "Driver").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users.Where(u => u.RoleId == roleId && u.IsActive);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
            var list = await q.OrderBy(u => u.FullName).Take(20).Select(u => new DriverDropdownDto
            {
                UserId = u.UserId,
                Display = u.FullName + " (" + u.Email + ")"
            }).ToListAsync();
            return ApiResponse<List<DriverDropdownDto>>.Ok(list);
        }

        public async Task<ApiResponse<CreatedUserResultDto>> ResetPasswordAsync(int userId)
        {
            var u = await _db.Users.FindAsync(userId);
            if (u is null) return ApiResponse<CreatedUserResultDto>.Fail("Driver not found.");
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
                Role = "Driver"
            }, "Password reset successfully.");
        }
    }
}
