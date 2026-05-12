using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Student;
using BusTracking.Common.DTOs.User;
using BusTracking.Common.Entities;
using BusTracking.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTracking.Common.Services
{
    public class StudentExtService : IStudentExtService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordService _pwd;
        private readonly IEmailService _email;
        public StudentExtService(AppDbContext db, IPasswordService pwd, IEmailService email)
        { _db = db; _pwd = pwd; _email = email; }

        public async Task<ApiResponse<StudentDetailViewDto>> GetDetailAsync(int studentId)
        {
            var s = await _db.Students
                .Include(x => x.User)
                .Include(x => x.Bus)
                .Include(x => x.Stop)
                .Include(x => x.ParentStudents)
                    .ThenInclude(ps => ps.Parent)
                        .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(x => x.StudentId == studentId);
            if (s is null) return ApiResponse<StudentDetailViewDto>.Fail("Student not found.");

            return ApiResponse<StudentDetailViewDto>.Ok(new StudentDetailViewDto
            {
                StudentId = s.StudentId,
                StudentCode = s.StudentCode,
                FullName = s.User.FullName,
                Email = s.User.Email,
                PhoneNumber = s.User.PhoneNumber,
                Standard = s.Standard,
                BusId = s.BusId,
                BusName = s.Bus?.BusName,
                BusNumber = s.Bus?.BusNumber,
                StopId = s.StopId,
                StopName = s.Stop?.StopName,
                IsActive = s.User.IsActive,
                CreatedAt = s.CreatedAt,
                ParentNames = s.ParentStudents.Select(ps => ps.Parent.User.FullName).ToList()
            });
        }

        public async Task<ApiResponse<CreatedUserResultDto>> CreateExtAsync(CreateStudentExtDto dto, int createdBy)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return ApiResponse<CreatedUserResultDto>.Fail("Email already in use.");
            if (await _db.Students.AnyAsync(s => s.StudentCode == dto.StudentCode))
                return ApiResponse<CreatedUserResultDto>.Fail("Student code already exists.");

            var roleId = await _db.Roles.Where(r => r.RoleName == "Student").Select(r => r.RoleId).FirstAsync();
            var password = _pwd.GenerateRandomPassword();
            var (hash, salt) = _pwd.HashPassword(password);

            var user = new User
            {
                RoleId = roleId,
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedBy = createdBy
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.Students.Add(new StudentDetail
            {
                UserId = user.UserId,
                StudentCode = dto.StudentCode,
                Standard = dto.Standard,
                BusId = dto.BusId,
                StopId = dto.StopId
            });
            await _db.SaveChangesAsync();

            bool emailSent = false;
            if (dto.SendWelcomeEmail)
            {
                try
                {
                    await _email.SendAsync(dto.Email, "Your Student Account",
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
            }, "Student created.");
        }

        public async Task<ApiResponse<bool>> UpdateExtAsync(int studentId, UpdateStudentExtDto dto)
        {
            var s = await _db.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.StudentId == studentId);
            if (s is null) return ApiResponse<bool>.Fail("Student not found.");

            s.User.FullName = dto.FullName;
            s.User.PhoneNumber = dto.PhoneNumber;
            s.User.IsActive = dto.IsActive;
            s.User.UpdatedAt = DateTime.UtcNow;
            s.Standard = dto.Standard;
            s.BusId = dto.BusId;
            s.StopId = dto.StopId;
            s.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Student updated.");
        }
    }
}
