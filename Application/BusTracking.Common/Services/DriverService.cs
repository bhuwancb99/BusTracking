namespace BusTracking.Common.Services
{
    public class DriverService : IDriverService
    {
        private readonly AppDbContext _db; private readonly IPasswordService _pwd; private readonly IEmailService _email;
        public DriverService(AppDbContext db, IPasswordService pwd, IEmailService email) { _db = db; _pwd = pwd; _email = email; }

        public async Task<ApiResponse<PagedResult<DriverListDto>>> GetAllAsync(int page, string? search, string? status)
        {
            var roleId = await _db.Roles.Where(r => r.RoleName == "Driver").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users.Include(u => u.DriverDetail).Where(u => u.RoleId == roleId);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(u => u.FullName.Contains(search) || u.UserName.Contains(search) || (u.Email != null && u.Email.Contains(search)));
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
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    LicenseNumber = u.DriverDetail != null ? u.DriverDetail.LicenseNumber : null,
                    LicenseExpiry = u.DriverDetail != null && u.DriverDetail.LicenseExpiry != null ? u.DriverDetail.LicenseExpiry.Value.ToString("yyyy-MM-dd") : null,
                    IsActive = u.IsActive,
                    ProfileImageUrl = u.ProfileImageUrl
                }).ToListAsync();

            var driverUserIds = items.Select(x => x.UserId).ToList();
            var driverBusMappings = await _db.BusDriverMappings
                .Where(bm => driverUserIds.Contains(bm.DriverUserId))
                .Include(bm => bm.Bus)
                .ToListAsync();

            var busGrouped = driverBusMappings
                .GroupBy(bm => bm.DriverUserId)
                .ToDictionary(
                    g => g.Key,
                    g => (
                        BusNumber: string.Join(", ", g.Select(x => x.Bus.BusNumber)),
                        BusName: string.Join(", ", g.Select(x => x.Bus.BusName))
                    )
                );

            foreach (var item in items)
            {
                if (busGrouped.TryGetValue(item.UserId, out var bInfo))
                {
                    item.BusNumber = bInfo.BusNumber;
                    item.BusName = bInfo.BusName;
                }
            }

            return ApiResponse<PagedResult<DriverListDto>>.Ok(new PagedResult<DriverListDto> { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }

        public Task<int> GetListPageSizeAsync() => PaginationHelper.GetListPageSizeAsync(_db);

        public async Task<ApiResponse<DriverListDto>> GetByIdAsync(int userId)
        {
            var u = await _db.Users.Include(x => x.DriverDetail).FirstOrDefaultAsync(x => x.UserId == userId);
            if (u is null) return ApiResponse<DriverListDto>.Fail("Not found.");

            var assignedBuses = await _db.BusDriverMappings
                .Where(bm => bm.DriverUserId == userId)
                .Include(bm => bm.Bus)
                .Select(bm => bm.Bus)
                .ToListAsync();

            var busNumber = assignedBuses.Any() ? string.Join(", ", assignedBuses.Select(b => b.BusNumber)) : null;
            var busName = assignedBuses.Any() ? string.Join(", ", assignedBuses.Select(b => b.BusName)) : null;

            return ApiResponse<DriverListDto>.Ok(new DriverListDto
            {
                UserId = u.UserId, FullName = u.FullName, UserName = u.UserName, Email = u.Email,
                PhoneNumber = u.PhoneNumber, LicenseNumber = u.DriverDetail?.LicenseNumber,
                LicenseExpiry = u.DriverDetail?.LicenseExpiry?.ToString("yyyy-MM-dd"), IsActive = u.IsActive, ProfileImageUrl = u.ProfileImageUrl,
                BusNumber = busNumber, BusName = busName
            });
        }

        public async Task<ApiResponse<CreatedUserResultDto>> CreateAsync(CreateDriverDto dto, int createdBy)
        {
            if (await _db.Users.AnyAsync(u => u.UserName == dto.UserName))
                return ApiResponse<CreatedUserResultDto>.Fail("Username already in use.");
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return ApiResponse<CreatedUserResultDto>.Fail("Email already in use.");

            var roleId = await _db.Roles.Where(r => r.RoleName == "Driver").Select(r => r.RoleId).FirstAsync();
            var password = !string.IsNullOrWhiteSpace(dto.Password) ? dto.Password : _pwd.GenerateRandomPassword();
            var (hash, salt) = _pwd.HashPassword(password);
            var user = new User
            {
                RoleId = roleId, FullName = dto.FullName, UserName = dto.UserName,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email,
                PhoneNumber = dto.PhoneNumber, PasswordHash = hash, PasswordSalt = salt, CreatedBy = createdBy
            };
            _db.Users.Add(user); await _db.SaveChangesAsync();
            _db.DriverDetails.Add(new DriverDetail
            {
                UserId = user.UserId, LicenseNumber = dto.LicenseNumber,
                LicenseExpiry = dto.LicenseExpiry is not null ? DateOnly.Parse(dto.LicenseExpiry) : null
            });
            await _db.SaveChangesAsync();
            if (dto.SendEmail && !string.IsNullOrWhiteSpace(dto.Email))
                await _email.SendAsync(dto.Email!, "Your Driver Account",
                    $"<p>Hi {dto.FullName},</p><p>Username: <b>{dto.UserName}</b><br/>Password: <b>{password}</b></p>");
            return ApiResponse<CreatedUserResultDto>.Ok(new CreatedUserResultDto
            { UserId = user.UserId, FullName = dto.FullName, UserName = dto.UserName, Email = dto.Email, PlainPassword = password, Role = "Driver" });
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateDriverDto dto)
        {
            var u = await _db.Users.Include(x => x.DriverDetail).FirstOrDefaultAsync(x => x.UserId == userId);
            if (u is null) return ApiResponse<bool>.Fail("Not found.");
            if (await _db.Users.AnyAsync(x => x.UserName == dto.UserName && x.UserId != userId))
                return ApiResponse<bool>.Fail("Username already in use.");
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.Users.AnyAsync(x => x.Email == dto.Email && x.UserId != userId))
                return ApiResponse<bool>.Fail("Email already in use.");

            u.FullName = dto.FullName; u.UserName = dto.UserName;
            u.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email;
            u.PhoneNumber = dto.PhoneNumber; u.IsActive = dto.IsActive; u.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            { var (hash, salt) = _pwd.HashPassword(dto.NewPassword); u.PasswordHash = hash; u.PasswordSalt = salt; }
            if (u.DriverDetail is not null)
            { u.DriverDetail.LicenseNumber = dto.LicenseNumber; u.DriverDetail.LicenseExpiry = dto.LicenseExpiry is not null ? DateOnly.Parse(dto.LicenseExpiry) : null; u.DriverDetail.UpdatedAt = DateTime.UtcNow; }
            await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Updated.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int userId)
        { var u = await _db.Users.FindAsync(userId); if (u is null) return ApiResponse<bool>.Fail("Not found."); u.IsActive = false; u.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Marked inactive."); }

        public async Task<ApiResponse<bool>> ToggleActiveAsync(int userId)
        { var u = await _db.Users.FindAsync(userId); if (u is null) return ApiResponse<bool>.Fail("Not found."); u.IsActive = !u.IsActive; u.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, u.IsActive ? "Activated." : "Deactivated."); }

        public async Task<ApiResponse<bool>> AssignBusAsync(AssignBusToDriverDto dto)
        {
            if (dto.BusId.HasValue)
            {
                if (!await _db.BusDriverMappings.AnyAsync(dm => dm.BusId == dto.BusId.Value && dm.DriverUserId == dto.DriverUserId))
                {
                    _db.BusDriverMappings.Add(new BusDriverMapping { BusId = dto.BusId.Value, DriverUserId = dto.DriverUserId });
                    await _db.SaveChangesAsync();
                }
            }
            return ApiResponse<bool>.Ok(true, "Bus assigned.");
        }

        public async Task<ApiResponse<List<DriverDropdownDto>>> GetDropdownAsync(string? search)
        {
            var roleId = await _db.Roles.Where(r => r.RoleName == "Driver").Select(r => r.RoleId).FirstAsync();
            var q = _db.Users.Where(u => u.RoleId == roleId && u.IsActive);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(u => u.FullName.Contains(search) || u.UserName.Contains(search));
            var list = await q.OrderBy(u => u.FullName).Select(u => new DriverDropdownDto
            { UserId = u.UserId, Display = u.FullName + " (" + u.UserName + ")" }).ToListAsync();
            return ApiResponse<List<DriverDropdownDto>>.Ok(list);
        }

        public async Task<ApiResponse<CreatedUserResultDto>> ResetPasswordAsync(int userId)
        {
            var u = await _db.Users.FindAsync(userId);
            if (u is null) return ApiResponse<CreatedUserResultDto>.Fail("Driver not found.");
            var newPassword = _pwd.GenerateRandomPassword();
            var (hash, salt) = _pwd.HashPassword(newPassword);
            u.PasswordHash = hash; u.PasswordSalt = salt; u.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<CreatedUserResultDto>.Ok(new CreatedUserResultDto
            { UserId = u.UserId, FullName = u.FullName, UserName = u.UserName, Email = u.Email, PlainPassword = newPassword, Role = "Driver" }, "Password reset successfully.");
        }
    }
}
