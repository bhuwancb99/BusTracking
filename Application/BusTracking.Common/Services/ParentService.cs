using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Parent;
using BusTracking.Common.Entities;
using BusTracking.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTracking.Common.Services
{
    public class ParentService : IParentService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordService _pwd;
        private readonly IEmailService _email;

        public ParentService(AppDbContext db, IPasswordService pwd, IEmailService email)
        { _db = db; _pwd = pwd; _email = email; }

        public async Task<ApiResponse<PagedResult<ParentListDto>>> GetAllAsync(int page, int pageSize, string? search)
        {
            var q = _db.Parents
                .Include(p => p.User)
                .Include(p => p.ParentStudents).ThenInclude(ps => ps.Student).ThenInclude(s => s.User)
                .Where(p => p.User.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(p => p.User.FullName.Contains(search) || p.User.Email.Contains(search));

            var total = await q.CountAsync();
            var items = await q.OrderBy(p => p.User.FullName)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ParentListDto
                {
                    UserId = p.UserId,
                    FullName = p.User.FullName,
                    Email = p.User.Email,
                    PhoneNumber = p.User.PhoneNumber,
                    IsActive = p.User.IsActive,
                    KidNames = p.ParentStudents.Select(ps => ps.Student.User.FullName).ToList()
                }).ToListAsync();

            return ApiResponse<PagedResult<ParentListDto>>.Ok(new PagedResult<ParentListDto>
            { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }

        public async Task<ApiResponse<ParentListDto>> GetByIdAsync(int userId)
        {
            var p = await _db.Parents
                .Include(x => x.User)
                .Include(x => x.ParentStudents).ThenInclude(ps => ps.Student).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            if (p is null) return ApiResponse<ParentListDto>.Fail("Parent not found.");

            return ApiResponse<ParentListDto>.Ok(new ParentListDto
            {
                UserId = p.UserId,
                FullName = p.User.FullName,
                Email = p.User.Email,
                PhoneNumber = p.User.PhoneNumber,
                IsActive = p.User.IsActive,
                KidNames = p.ParentStudents.Select(ps => ps.Student.User.FullName).ToList()
            });
        }

        public async Task<ApiResponse<bool>> CreateAsync(CreateParentDto dto, int createdBy)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return ApiResponse<bool>.Fail("Email already in use.");

            var roleId = await _db.Roles.Where(r => r.RoleName == "Parent").Select(r => r.RoleId).FirstAsync();
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

            var parent = new ParentDetail { UserId = user.UserId };
            _db.Parents.Add(parent);
            await _db.SaveChangesAsync();

            // Link students by StudentCode
            foreach (var code in dto.StudentCodes)
            {
                var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentCode == code);
                if (student is not null)
                    _db.ParentStudents.Add(new ParentStudent { ParentId = parent.ParentId, StudentId = student.StudentId });
            }
            await _db.SaveChangesAsync();

            await _email.SendAsync(dto.Email, "Your Parent Account - Bus Tracking",
                $"<p>Hi {dto.FullName},</p><p>Your parent account has been created.<br/>Email: {dto.Email}<br/>Password: <b>{password}</b></p>");

            return ApiResponse<bool>.Ok(true, "Parent created.");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateParentDto dto)
        {
            var p = await _db.Parents.Include(x => x.User).Include(x => x.ParentStudents)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            if (p is null) return ApiResponse<bool>.Fail("Parent not found.");

            p.User.FullName = dto.FullName;
            p.User.PhoneNumber = dto.PhoneNumber;
            p.User.UpdatedAt = DateTime.UtcNow;

            _db.ParentStudents.RemoveRange(p.ParentStudents);
            foreach (var code in dto.StudentCodes)
            {
                var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentCode == code);
                if (student is not null)
                    _db.ParentStudents.Add(new ParentStudent { ParentId = p.ParentId, StudentId = student.StudentId });
            }
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Parent updated.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int userId)
        {
            var p = await _db.Parents.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == userId);
            if (p is null) return ApiResponse<bool>.Fail("Parent not found.");
            p.User.IsActive = false;
            p.User.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Parent deleted.");
        }
    }
}
