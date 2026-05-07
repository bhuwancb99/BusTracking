using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.User;
using BusTracking.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTracking.Common.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        public UserService(AppDbContext db) => _db = db;

        public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId)
        {
            var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
            if (user is null) return ApiResponse<UserProfileDto>.Fail("User not found.");

            return ApiResponse<UserProfileDto>.Ok(new UserProfileDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                Role = user.Role.RoleName
            });
        }

        public async Task<ApiResponse<bool>> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null) return ApiResponse<bool>.Fail("User not found.");
            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Profile updated.");
        }
    }
}
