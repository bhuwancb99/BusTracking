namespace BusTracking.Common.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IJwtService _jwt;
        private readonly IPasswordService _pwd;
        private readonly IEmailService _email;

        public AuthService(AppDbContext db, IJwtService jwt, IPasswordService pwd, IEmailService email)
        {
            _db = db;
            _jwt = jwt;
            _pwd = pwd;
            _email = email;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

            if (user is null || !_pwd.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
                return ApiResponse<LoginResponseDto>.Fail("Invalid email or password.");

            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Load permissions for BusCoordinator — all other roles either have
            // no permission system (Driver/Parent/Student) or have all permissions (SuperAdmin).
            var permissions = "";
            if (user.Role.RoleName == "BusCoordinator")
            {
                var keys = await GetCoordinatorPermissionsAsync(user.UserId);
                permissions = System.Text.Json.JsonSerializer.Serialize(keys);
            }

            var token = _jwt.GenerateToken(user.UserId, user.Email, user.Role.RoleName);
            return ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.RoleName,
                Token = token,
                Expiry = DateTime.UtcNow.AddHours(8),
                Permissions = permissions
            });
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);
            if (user is null)
                return ApiResponse<bool>.Ok(true, "If that email exists, a reset link has been sent.");

            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                               .Replace("+", "-").Replace("/", "_").TrimEnd('=');

            _db.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.UserId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            });
            await _db.SaveChangesAsync();

            var resetLink = $"https://yourdomain.com/auth/reset-password?token={token}";
            await _email.SendAsync(user.Email, "Reset Your Password",
                $"<p>Hi {user.FullName},</p><p>Click <a href='{resetLink}'>here</a> to reset your password. Link expires in 2 hours.</p>");

            return ApiResponse<bool>.Ok(true, "Reset link sent to your email.");
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return ApiResponse<bool>.Fail("Passwords do not match.");

            var tokenRecord = await _db.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == dto.Token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

            if (tokenRecord is null)
                return ApiResponse<bool>.Fail("Invalid or expired reset token.");

            var (hash, salt) = _pwd.HashPassword(dto.NewPassword);
            tokenRecord.User.PasswordHash = hash;
            tokenRecord.User.PasswordSalt = salt;
            tokenRecord.User.UpdatedAt = DateTime.UtcNow;
            tokenRecord.IsUsed = true;
            await _db.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Password reset successfully.");
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return ApiResponse<bool>.Fail("Passwords do not match.");

            var user = await _db.Users.FindAsync(userId);
            if (user is null) return ApiResponse<bool>.Fail("User not found.");

            if (!_pwd.VerifyPassword(dto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                return ApiResponse<bool>.Fail("Current password is incorrect.");

            var (hash, salt) = _pwd.HashPassword(dto.NewPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Password changed.");
        }

        public async Task<List<string>> GetCoordinatorPermissionsAsync(int userId)
        {
            return await _db.SubAdminPermissions
                .Where(sp => sp.UserId == userId)
                .Select(sp => sp.Permission.PermissionKey)
                .ToListAsync();
        }
    }
}