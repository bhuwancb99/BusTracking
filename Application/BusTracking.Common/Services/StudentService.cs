namespace BusTracking.Common.Services
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _db; private readonly IPasswordService _pwd; private readonly IEmailService _email;
        public StudentService(AppDbContext db, IPasswordService pwd, IEmailService email) { _db = db; _pwd = pwd; _email = email; }

        public async Task<ApiResponse<PagedResult<StudentListDto>>> GetAllAsync(int page, string? search, string? status)
        {
            var q = _db.Students.Include(s => s.User).Include(s => s.Bus).Include(s => s.Stop).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(s => s.User.FullName.Contains(search) || s.User.UserName.Contains(search) || s.StudentCode.Contains(search));
            if (status == "Active") q = q.Where(s => s.User.IsActive);
            else if (status == "Inactive") q = q.Where(s => !s.User.IsActive);

            var pageSize = await GetListPageSizeAsync();
            page = PaginationHelper.Clamp(page);

            var total = await q.CountAsync();
            var items = await q.OrderBy(s => s.User.FullName).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(s => new StudentListDto
                {
                    StudentId = s.StudentId,
                    UserId = s.UserId,
                    StudentCode = s.StudentCode,
                    FullName = s.User.FullName,
                    Email = s.User.Email,
                    PhoneNumber = s.User.PhoneNumber,
                    Standard = s.Standard,
                    BusId = s.BusId,
                    BusName = s.Bus != null ? s.Bus.BusName : null,
                    BusNumber = s.Bus != null ? s.Bus.BusNumber : null,
                    StopId = s.StopId,
                    StopName = s.Stop != null ? s.Stop.StopName : null,
                    IsActive = s.User.IsActive
                }).ToListAsync();
            return ApiResponse<PagedResult<StudentListDto>>.Ok(new PagedResult<StudentListDto>
            {
                Items = items,
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize
            });
        }

        public async Task<int> GetListPageSizeAsync()
        {
            var raw = await _db.AppConfigurations
                .Where(c => c.ConfigKey == AppConstants.AppConfigPageSizeKey && c.IsActive)
                .Select(c => c.ConfigValue)
                .FirstOrDefaultAsync();

            return int.TryParse(raw, out var size) && size > 0
                ? PaginationHelper.ClampPageSize(size)
                : AppConstants.DefaultPageSize;
        }
        public async Task<ApiResponse<StudentListDto>> GetByIdAsync(int studentId)
        {
            var s = await _db.Students.Include(x => x.User).Include(x => x.Bus).Include(x => x.Stop).FirstOrDefaultAsync(x => x.StudentId == studentId);
            if (s is null) return ApiResponse<StudentListDto>.Fail("Not found.");
            return ApiResponse<StudentListDto>.Ok(new StudentListDto
            {
                StudentId = s.StudentId,
                UserId = s.UserId,
                StudentCode = s.StudentCode,
                FullName = s.User.FullName,
                UserName = s.User.UserName,
                Email = s.User.Email,
                PhoneNumber = s.User.PhoneNumber,
                Standard = s.Standard,
                BusId = s.BusId,
                BusName = s.Bus?.BusName,
                BusNumber = s.Bus?.BusNumber,
                StopId = s.StopId,
                StopName = s.Stop?.StopName,
                IsActive = s.User.IsActive,
                ProfileImageUrl = s.User.ProfileImageUrl
            });
        }
        public async Task<ApiResponse<CreatedUserResultDto>> CreateAsync(CreateStudentDto dto, int createdBy)
        {
            if (await _db.Users.AnyAsync(u => u.UserName == dto.UserName))
                return ApiResponse<CreatedUserResultDto>.Fail("Username already in use.");
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return ApiResponse<CreatedUserResultDto>.Fail("Email in use.");
            if (await _db.Students.AnyAsync(s => s.StudentCode == dto.StudentCode)) return ApiResponse<CreatedUserResultDto>.Fail("Student code exists.");
            var roleId = await _db.Roles.Where(r => r.RoleName == "Student").Select(r => r.RoleId).FirstAsync();
            var password = !string.IsNullOrWhiteSpace(dto.Password) ? dto.Password : _pwd.GenerateRandomPassword();
            var (hash, salt) = _pwd.HashPassword(password);
            var user = new User
            {
                RoleId = roleId,
                FullName = dto.FullName,
                UserName = dto.UserName,
                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedBy = createdBy
            };
            _db.Users.Add(user); await _db.SaveChangesAsync();
            _db.Students.Add(new StudentDetail
            {
                UserId = user.UserId,
                StudentCode = dto.StudentCode,
                Standard = dto.Standard,
                BusId = dto.BusId,
                StopId = dto.StopId
            });
            await _db.SaveChangesAsync();
            if (dto.SendEmail && !string.IsNullOrWhiteSpace(dto.Email)) await _email.SendAsync(dto.Email!, "Your Student Account", $"<p>Hi {dto.FullName},</p><p>Username: <b>{dto.UserName}</b><br/>Password: <b>{password}</b></p>");
            return ApiResponse<CreatedUserResultDto>.Ok(new CreatedUserResultDto
            {
                UserId = user.UserId,
                FullName = dto.FullName,
                UserName = dto.UserName,
                Email = dto.Email,
                PlainPassword = password,
                Role = "Student"
            });
        }
        public async Task<ApiResponse<bool>> UpdateAsync(int studentId, UpdateStudentDto dto)
        {
            var s = await _db.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.StudentId == studentId);
            if (s is null) return ApiResponse<bool>.Fail("Not found.");
            if (await _db.Users.AnyAsync(u => u.UserName == dto.UserName && u.UserId != s.UserId))
                return ApiResponse<bool>.Fail("Username already in use.");
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != s.UserId))
                return ApiResponse<bool>.Fail("Email already in use.");
            s.User.FullName = dto.FullName;
            s.User.UserName = dto.UserName;
            s.User.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email;
            s.User.PhoneNumber = dto.PhoneNumber;
            s.User.IsActive = dto.IsActive;
            s.User.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var (hash, salt) = _pwd.HashPassword(dto.NewPassword);
                s.User.PasswordHash = hash;
                s.User.PasswordSalt = salt;
            }
            s.StudentCode = dto.StudentCode;
            s.Standard = dto.Standard;
            s.BusId = dto.BusId;
            s.StopId = dto.StopId;
            s.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Updated.");
        }
        public async Task<ApiResponse<bool>> DeleteAsync(int studentId)
        { var s = await _db.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.StudentId == studentId); if (s is null) return ApiResponse<bool>.Fail("Not found."); s.User.IsActive = false; s.User.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Marked inactive."); }
        public async Task<ApiResponse<bool>> ToggleActiveAsync(int studentId)
        { var s = await _db.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.StudentId == studentId); if (s is null) return ApiResponse<bool>.Fail("Not found."); s.User.IsActive = !s.User.IsActive; s.User.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, s.User.IsActive ? "Activated." : "Deactivated."); }
        public async Task<ApiResponse<bool>> AssignBusAsync(AssignBusToStudentDto dto)
        { var s = await _db.Students.FindAsync(dto.StudentId); if (s is null) return ApiResponse<bool>.Fail("Not found."); s.BusId = dto.BusId; s.StopId = dto.StopId; s.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Bus assigned."); }
        public async Task<ApiResponse<bool>> SetAvailabilityAsync(CreateAvailabilityDto dto, int markedBy)
        {
            if (!DateOnly.TryParse(dto.FromDate, out var from) || !DateOnly.TryParse(dto.ToDate, out var to)) return ApiResponse<bool>.Fail("Invalid date.");
            _db.StudentAvailabilities.Add(new StudentAvailability
            {
                StudentId = dto.StudentId,
                AvailabilityType = dto.AvailabilityType,
                FromDate = from,
                ToDate = to,
                Remarks = dto.Remarks,
                MarkedBy = markedBy
            });
            await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Saved.");
        }
        public async Task<ApiResponse<List<AvailabilityDto>>> GetAvailabilitiesAsync(int studentId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var list = await _db.StudentAvailabilities.Where(a => a.StudentId == studentId && a.ToDate >= today).OrderBy(a => a.FromDate)
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
        public async Task<ApiResponse<List<StudentSearchDto>>> SearchAsync(string? query)
        {
            var q = _db.Students.Include(s => s.User).Include(s => s.Bus).Where(s => s.User.IsActive);
            if (!string.IsNullOrWhiteSpace(query)) q = q.Where(s => s.User.FullName.Contains(query) || s.StudentCode.Contains(query));
            var list = await q.OrderBy(s => s.User.FullName).Take(10).Select(s => new StudentSearchDto
            {
                StudentId = s.StudentId,
                StudentCode = s.StudentCode,
                FullName = s.User.FullName,
                Standard = s.Standard,
                BusNumber = s.Bus != null ? s.Bus.BusNumber : null
            }).ToListAsync();
            return ApiResponse<List<StudentSearchDto>>.Ok(list);
        }

        public async Task<ApiResponse<CreatedUserResultDto>> ResetPasswordAsync(int studentId)
        {
            var u = await _db.Users.FindAsync(studentId);
            if (u is null) return ApiResponse<CreatedUserResultDto>.Fail("Student not found.");
            var newPassword = _pwd.GenerateRandomPassword();
            var (hash, salt) = _pwd.HashPassword(newPassword);
            u.PasswordHash = hash;
            u.PasswordSalt = salt;
            u.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<CreatedUserResultDto>.Ok(new CreatedUserResultDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PlainPassword = newPassword,
                Role = "Student"
            }, "Password reset successfully.");
        }
    }
}
