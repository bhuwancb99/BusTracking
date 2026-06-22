namespace BusTracking.Common.Services
{
    public class DriverExtService : IDriverExtService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordService _pwd;
        private readonly IEmailService _email;
        public DriverExtService(AppDbContext db, IPasswordService pwd, IEmailService email)
        { _db = db; _pwd = pwd; _email = email; }

        public async Task<ApiResponse<DriverDetailViewDto>> GetDetailAsync(int userId)
        {
            var u = await _db.Users
                .Include(x => x.DriverDetail)
                    .ThenInclude(d => d!.Bus)
                        .ThenInclude(b => b!.Route)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            if (u is null) return ApiResponse<DriverDetailViewDto>.Fail("Driver not found.");

            return ApiResponse<DriverDetailViewDto>.Ok(new DriverDetailViewDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                LicenseNumber = u.DriverDetail?.LicenseNumber,
                LicenseExpiry = u.DriverDetail?.LicenseExpiry?.ToString("yyyy-MM-dd"),
                BusId = u.DriverDetail?.BusId,
                BusName = u.DriverDetail?.Bus?.BusName,
                BusNumber = u.DriverDetail?.Bus?.BusNumber,
                RouteName = u.DriverDetail?.Bus?.Route?.RouteName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            });
        }

        public async Task<ApiResponse<CreatedUserResultDto>> CreateExtAsync(CreateDriverExtDto dto, int createdBy)
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
                RoleId = roleId,
                FullName = dto.FullName,
                UserName = dto.UserName,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email,
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

            bool emailSent = false;
            if (dto.SendWelcomeEmail && !string.IsNullOrWhiteSpace(dto.Email))
            {
                try
                {
                    await _email.SendAsync(dto.Email!, "Your Driver Account",
                        $"<p>Hi {dto.FullName},</p><p>Username: <b>{dto.UserName}</b><br>Password: <b>{password}</b></p>");
                    emailSent = true;
                }
                catch { }
            }

            return ApiResponse<CreatedUserResultDto>.Ok(new CreatedUserResultDto
            {
                UserId = user.UserId,
                FullName = dto.FullName,
                UserName = dto.UserName,
                Email = dto.Email,
                GeneratedPassword = password,
                PlainPassword = password,
                EmailSent = emailSent
            }, "Driver created successfully.");
        }

        public async Task<ApiResponse<bool>> UpdateExtAsync(int userId, UpdateDriverExtDto dto)
        {
            var user = await _db.Users.Include(u => u.DriverDetail).FirstOrDefaultAsync(u => u.UserId == userId);
            if (user is null) return ApiResponse<bool>.Fail("Driver not found.");

            if (await _db.Users.AnyAsync(u => u.UserName == dto.UserName && u.UserId != userId))
                return ApiResponse<bool>.Fail("Username already in use.");
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != userId))
                return ApiResponse<bool>.Fail("Email already in use.");

            user.FullName = dto.FullName;
            user.UserName = dto.UserName;
            user.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var (hash, salt) = _pwd.HashPassword(dto.NewPassword);
                user.PasswordHash = hash; user.PasswordSalt = salt;
            }

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

        public async Task<ApiResponse<bool>> ToggleActiveAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return ApiResponse<bool>.Fail("Driver not found.");
            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, user.IsActive ? "Driver activated." : "Driver deactivated.");
        }
    }
}
