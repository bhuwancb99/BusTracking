namespace BusTracking.Common.Services
{
    public class SubAdminExtService : ISubAdminExtService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordService _pwd;
        private readonly IEmailService _email;
        public SubAdminExtService(AppDbContext db, IPasswordService pwd, IEmailService email)
        { _db = db; _pwd = pwd; _email = email; }

        public async Task<ApiResponse<CreatedUserResultDto>> CreateExtAsync(CreateSubAdminExtDto dto, int createdBy)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return ApiResponse<CreatedUserResultDto>.Fail("Email already in use.");

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

            foreach (var pid in dto.PermissionIds.Distinct())
                _db.SubAdminPermissions.Add(new SubAdminPermission
                { UserId = user.UserId, PermissionId = pid, AssignedBy = createdBy });
            await _db.SaveChangesAsync();

            bool emailSent = false;
            if (dto.SendWelcomeEmail)
            {
                try
                {
                    await _email.SendAsync(dto.Email, "Your Coordinator Account",
                        $"<p>Hi {dto.FullName},</p><p>Email: <b>{dto.Email}</b><br>Password: <b>{password}</b></p>");
                    emailSent = true;
                }
                catch { }
            }

            return ApiResponse<CreatedUserResultDto>.Ok(new CreatedUserResultDto
            {
                UserId = user.UserId,
                FullName = dto.FullName,
                Email = dto.Email,
                GeneratedPassword = password,
                EmailSent = emailSent
            }, "Coordinator created.");
        }

        public async Task<ApiResponse<bool>> UpdateExtAsync(int userId, UpdateSubAdminExtDto dto)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return ApiResponse<bool>.Fail("Coordinator not found.");

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            var existing = _db.SubAdminPermissions.Where(sp => sp.UserId == userId);
            _db.SubAdminPermissions.RemoveRange(existing);
            foreach (var pid in dto.PermissionIds.Distinct())
                _db.SubAdminPermissions.Add(new SubAdminPermission
                { UserId = userId, PermissionId = pid, AssignedBy = userId });

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Coordinator updated.");
        }
    }
}
