using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Parent;
using BusTracking.Common.DTOs.User;
using BusTracking.Common.Entities;
using BusTracking.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTracking.Common.Services
{
    public class ParentExtService : IParentExtService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordService _pwd;
        private readonly IEmailService _email;
        public ParentExtService(AppDbContext db, IPasswordService pwd, IEmailService email)
        { _db = db; _pwd = pwd; _email = email; }

        public async Task<ApiResponse<ParentDetailViewDto>> GetDetailAsync(int userId)
        {
            var p = await _db.Parents
                .Include(x => x.User)
                .Include(x => x.ParentStudents)
                    .ThenInclude(ps => ps.Student)
                        .ThenInclude(s => s.User)
                .Include(x => x.ParentStudents)
                    .ThenInclude(ps => ps.Student)
                        .ThenInclude(s => s.Bus)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            if (p is null) return ApiResponse<ParentDetailViewDto>.Fail("Parent not found.");

            return ApiResponse<ParentDetailViewDto>.Ok(new ParentDetailViewDto
            {
                UserId = p.UserId,
                FullName = p.User.FullName,
                Email = p.User.Email,
                PhoneNumber = p.User.PhoneNumber,
                IsActive = p.User.IsActive,
                CreatedAt = p.CreatedAt,
                Students = p.ParentStudents.Select(ps => new LinkedStudentDto
                {
                    StudentId = ps.Student.StudentId,
                    StudentCode = ps.Student.StudentCode,
                    FullName = ps.Student.User.FullName,
                    Standard = ps.Student.Standard,
                    BusNumber = ps.Student.Bus?.BusNumber
                }).ToList()
            });
        }

        public async Task<ApiResponse<CreatedUserResultDto>> CreateExtAsync(CreateParentExtDto dto, int createdBy)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return ApiResponse<CreatedUserResultDto>.Fail("Email already in use.");

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

            foreach (var code in dto.StudentCodes.Where(c => !string.IsNullOrWhiteSpace(c)))
            {
                var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentCode == code.Trim());
                if (student is not null)
                    _db.ParentStudents.Add(new ParentStudent { ParentId = parent.ParentId, StudentId = student.StudentId });
            }
            await _db.SaveChangesAsync();

            bool emailSent = false;
            if (dto.SendWelcomeEmail)
            {
                try
                {
                    await _email.SendAsync(dto.Email, "Your Parent Account",
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
            }, "Parent created.");
        }

        public async Task<ApiResponse<bool>> UpdateExtAsync(int userId, UpdateParentExtDto dto)
        {
            var p = await _db.Parents.Include(x => x.User).Include(x => x.ParentStudents)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            if (p is null) return ApiResponse<bool>.Fail("Parent not found.");

            p.User.FullName = dto.FullName;
            p.User.PhoneNumber = dto.PhoneNumber;
            p.User.IsActive = dto.IsActive;
            p.User.UpdatedAt = DateTime.UtcNow;

            // Replace student links
            _db.ParentStudents.RemoveRange(p.ParentStudents);
            foreach (var code in dto.StudentCodes.Where(c => !string.IsNullOrWhiteSpace(c)))
            {
                var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentCode == code.Trim());
                if (student is not null)
                    _db.ParentStudents.Add(new ParentStudent { ParentId = p.ParentId, StudentId = student.StudentId });
            }
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Parent updated.");
        }
    }
}
