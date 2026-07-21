namespace BusTracking.Common.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        public UserService(AppDbContext db) => _db = db;
        public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId)
        {
            var u = await _db.Users.Include(x => x.Role).Include(x => x.School).FirstOrDefaultAsync(x => x.UserId == userId);
            if (u is null) return ApiResponse<UserProfileDto>.Fail("User not found.");
            return ApiResponse<UserProfileDto>.Ok(new UserProfileDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                ProfileImageUrl = u.ProfileImageUrl,
                Role = u.Role.RoleName,
                IsActive = u.IsActive,
                SchoolId = u.SchoolId,
                SchoolName = u.School?.SchoolName,
                SchoolLogo = u.School?.SchoolLogo
            });
        }
        public async Task<ApiResponse<bool>> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var u = await _db.Users.FindAsync(userId);
            if (u is null) return ApiResponse<bool>.Fail("User not found.");
            u.FullName = dto.FullName; u.PhoneNumber = dto.PhoneNumber; u.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Profile updated.");
        }
    }
}
