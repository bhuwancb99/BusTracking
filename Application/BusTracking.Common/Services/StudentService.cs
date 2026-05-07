using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Availability;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Student;
using BusTracking.Common.Entities;
using BusTracking.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTracking.Common.Services
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordService _pwd;
        private readonly IEmailService _email;
        public StudentService(AppDbContext db, IPasswordService pwd, IEmailService email)
        { _db = db; _pwd = pwd; _email = email; }

        public async Task<ApiResponse<PagedResult<StudentListDto>>> GetAllAsync(int page, int pageSize, string? search)
        {
            var q = _db.Students
                .Include(s => s.User)
                .Include(s => s.Bus)
                .Include(s => s.Stop)
                .Where(s => s.User.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(s => s.User.FullName.Contains(search) || s.StudentCode.Contains(search));

            var total = await q.CountAsync();
            var items = await q.OrderBy(s => s.User.FullName)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(s => new StudentListDto
                {
                    StudentId = s.StudentId,
                    StudentCode = s.StudentCode,
                    FullName = s.User.FullName,
                    Email = s.User.Email,
                    Standard = s.Standard,
                    BusNumber = s.Bus != null ? s.Bus.BusNumber : null,
                    StopName = s.Stop != null ? s.Stop.StopName : null,
                    IsActive = s.User.IsActive
                }).ToListAsync();

            return ApiResponse<PagedResult<StudentListDto>>.Ok(new PagedResult<StudentListDto>
            { Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize });
        }

        public async Task<ApiResponse<StudentListDto>> GetByIdAsync(int studentId)
        {
            var s = await _db.Students
                .Include(x => x.User).Include(x => x.Bus).Include(x => x.Stop)
                .FirstOrDefaultAsync(x => x.StudentId == studentId);

            if (s is null) return ApiResponse<StudentListDto>.Fail("Student not found.");
            return ApiResponse<StudentListDto>.Ok(new StudentListDto
            {
                StudentId = s.StudentId,
                StudentCode = s.StudentCode,
                FullName = s.User.FullName,
                Email = s.User.Email,
                Standard = s.Standard,
                BusNumber = s.Bus?.BusNumber,
                StopName = s.Stop?.StopName,
                IsActive = s.User.IsActive
            });
        }

        public async Task<ApiResponse<bool>> CreateAsync(CreateStudentDto dto, int createdBy)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return ApiResponse<bool>.Fail("Email already in use.");
            if (await _db.Students.AnyAsync(s => s.StudentCode == dto.StudentCode))
                return ApiResponse<bool>.Fail("Student code already exists.");

            var studentRoleId = await _db.Roles.Where(r => r.RoleName == "Student").Select(r => r.RoleId).FirstAsync();
            var password = _pwd.GenerateRandomPassword();
            var (hash, salt) = _pwd.HashPassword(password);

            var user = new User
            {
                RoleId = studentRoleId,
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

            await _email.SendAsync(dto.Email, "Your Bus Tracking Account",
                $"<p>Hi {dto.FullName},</p><p>Your account has been created.<br/>Email: {dto.Email}<br/>Password: <b>{password}</b></p><p>Please change your password after logging in.</p>");

            return ApiResponse<bool>.Ok(true, "Student created.");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int studentId, UpdateStudentDto dto)
        {
            var student = await _db.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student is null) return ApiResponse<bool>.Fail("Student not found.");

            student.User.FullName = dto.FullName;
            student.User.UpdatedAt = DateTime.UtcNow;
            student.Standard = dto.Standard;
            student.BusId = dto.BusId;
            student.StopId = dto.StopId;
            student.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Student updated.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int studentId)
        {
            var student = await _db.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.StudentId == studentId);
            if (student is null) return ApiResponse<bool>.Fail("Student not found.");
            student.User.IsActive = false;
            student.User.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Student deleted.");
        }

        public async Task<ApiResponse<bool>> SetAvailabilityAsync(CreateAvailabilityDto dto, int markedBy)
        {
            if (!DateOnly.TryParse(dto.FromDate, out var from) || !DateOnly.TryParse(dto.ToDate, out var to))
                return ApiResponse<bool>.Fail("Invalid date format.");

            _db.StudentAvailabilities.Add(new StudentAvailability
            {
                StudentId = dto.StudentId,
                AvailabilityType = dto.AvailabilityType,
                FromDate = from,
                ToDate = to,
                Remarks = dto.Remarks,
                MarkedBy = markedBy
            });
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Availability saved.");
        }

        public async Task<ApiResponse<List<AvailabilityDto>>> GetAvailabilitiesAsync(int studentId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var list = await _db.StudentAvailabilities
                .Where(a => a.StudentId == studentId && a.ToDate >= today)
                .OrderBy(a => a.FromDate)
                .Select(a => new AvailabilityDto
                {
                    AvailabilityId = a.AvailabilityId,
                    AvailabilityType = a.AvailabilityType.ToString(),
                    FromDate = a.FromDate.ToString("yyyy-MM-dd"),
                    ToDate = a.ToDate.ToString("yyyy-MM-dd"),
                    Remarks = a.Remarks
                }).ToListAsync();

            return ApiResponse<List<AvailabilityDto>>.Ok(list);
        }
    }
}
