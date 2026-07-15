namespace BusTracking.Common.Services
{
    public class ParentService : IParentService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordService _pwd;
        private readonly IEmailService _email;
        public ParentService(AppDbContext db, IPasswordService pwd, IEmailService email)
        {
            _db = db;
            _pwd = pwd;
            _email = email;
        }

        public async Task<ApiResponse<PagedResult<ParentListDto>>> GetAllAsync(int page, string? search, string? status)
        {
            var q = _db.Parents.Include(p => p.User).Include(p => p.ParentStudents).ThenInclude(ps => ps.Student).ThenInclude(s => s.User).Include(p => p.ParentStudents).ThenInclude(ps => ps.Student).ThenInclude(s => s.Standard).Include(p => p.ParentStudents).ThenInclude(ps => ps.Student).ThenInclude(s => s.Bus).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(p => p.User.FullName.Contains(search) || p.User.UserName.Contains(search) || (p.User.Email != null && p.User.Email.Contains(search)));
            if (status == "Active") q = q.Where(p => p.User.IsActive);
            else if (status == "Inactive") q = q.Where(p => !p.User.IsActive);

            var pageSize = await GetListPageSizeAsync();
            page = PaginationHelper.Clamp(page);

            var total = await q.CountAsync();
            var items = await q.OrderBy(p => p.User.FullName).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(p => new ParentListDto
                {
                    UserId = p.UserId,
                    FullName = p.User.FullName,
                    Email = p.User.Email,
                    PhoneNumber = p.User.PhoneNumber,
                    IsActive = p.User.IsActive,
                    Students = p.ParentStudents.Select(ps => new LinkedStudentDto
                    {
                        StudentId = ps.Student.StudentId,
                        StudentCode = ps.Student.StudentCode,
                        FullName = ps.Student.User.FullName,
                        StandardId = ps.Student.StandardId,
                        StandardName = ps.Student.Standard != null ? ps.Student.Standard.StandardName : null,
                        BusNumber = ps.Student.Bus != null ? ps.Student.Bus.BusNumber : null,
                        BusName = ps.Student.Bus != null ? ps.Student.Bus.BusName : null
                    }).ToList()
                }).ToListAsync();
            return ApiResponse<PagedResult<ParentListDto>>.Ok(new PagedResult<ParentListDto>
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
        public async Task<ApiResponse<ParentListDto>> GetByIdAsync(int userId)
        {
            var p = await _db.Parents.Include(x => x.User).Include(x => x.ParentStudents).ThenInclude(ps => ps.Student).ThenInclude(s => s.User).Include(x => x.ParentStudents).ThenInclude(ps => ps.Student).ThenInclude(s => s.Standard).Include(x => x.ParentStudents).ThenInclude(ps => ps.Student).ThenInclude(s => s.Bus).FirstOrDefaultAsync(x => x.UserId == userId);
            if (p is null)
                return ApiResponse<ParentListDto>.Fail("Not found.");
            return ApiResponse<ParentListDto>.Ok(new ParentListDto
            {
                UserId = p.UserId,
                FullName = p.User.FullName,
                UserName = p.User.UserName,
                Email = p.User.Email,
                PhoneNumber = p.User.PhoneNumber,
                IsActive = p.User.IsActive,
                ProfileImageUrl = p.User.ProfileImageUrl,
                Students = p.ParentStudents.Select(ps => new LinkedStudentDto
                {
                    StudentId = ps.Student.StudentId,
                    StudentCode = ps.Student.StudentCode,
                    FullName = ps.Student.User.FullName,
                    StandardId = ps.Student.StandardId,
                    StandardName = ps.Student.Standard?.StandardName,
                    BusNumber = ps.Student.Bus?.BusNumber,
                    BusName = ps.Student.Bus?.BusName
                }).ToList()
            });
        }
        public async Task<ApiResponse<CreatedUserResultDto>> CreateAsync(CreateParentDto dto, int createdBy)
        {
            if (await _db.Users.AnyAsync(u => u.UserName == dto.UserName))
                return ApiResponse<CreatedUserResultDto>.Fail("Username already in use.");
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return ApiResponse<CreatedUserResultDto>.Fail("Email in use.");
            var roleId = await _db.Roles.Where(r => r.RoleName == "Parent").Select(r => r.RoleId).FirstAsync();
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
            _db.Users.Add(user); await _db.SaveChangesAsync();
            var parent = new ParentDetail
            {
                UserId = user.UserId
            }; _db.Parents.Add(parent); await _db.SaveChangesAsync();
            foreach (var code in dto.StudentCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct())
            { var s = await _db.Students.FirstOrDefaultAsync(x => x.StudentCode == code.Trim()); if (s is not null) _db.ParentStudents.Add(new ParentStudent { ParentId = parent.ParentId, StudentId = s.StudentId }); }
            await _db.SaveChangesAsync();
            if (dto.SendEmail && !string.IsNullOrWhiteSpace(dto.Email)) await _email.SendAsync(dto.Email!, "Your Parent Account", $"<p>Hi {dto.FullName},</p><p>Username: <b>{dto.UserName}</b><br/>Password: <b>{password}</b></p>");
            return ApiResponse<CreatedUserResultDto>.Ok(new CreatedUserResultDto
            {
                UserId = user.UserId,
                FullName = dto.FullName,
                UserName = dto.UserName,
                Email = dto.Email,
                PlainPassword = password,
                Role = "Parent"
            });
        }
        public async Task<ApiResponse<bool>> UpdateAsync(int userId, UpdateParentDto dto)
        {
            var p = await _db.Parents.Include(x => x.User).Include(x => x.ParentStudents).FirstOrDefaultAsync(x => x.UserId == userId);
            if (p is null) return ApiResponse<bool>.Fail("Not found.");
            if (await _db.Users.AnyAsync(x => x.UserName == dto.UserName && x.UserId != userId))
                return ApiResponse<bool>.Fail("Username already in use.");
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.Users.AnyAsync(x => x.Email == dto.Email && x.UserId != userId))
                return ApiResponse<bool>.Fail("Email already in use.");

            p.User.FullName = dto.FullName;
            p.User.UserName = dto.UserName;
            p.User.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email;
            p.User.PhoneNumber = dto.PhoneNumber;
            p.User.IsActive = dto.IsActive;
            p.User.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var (hash, salt) = _pwd.HashPassword(dto.NewPassword);
                p.User.PasswordHash = hash;
                p.User.PasswordSalt = salt;
            }
            _db.ParentStudents.RemoveRange(p.ParentStudents);
            foreach (var code in dto.StudentCodes.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct())
            { var s = await _db.Students.FirstOrDefaultAsync(x => x.StudentCode == code.Trim()); if (s is not null) _db.ParentStudents.Add(new ParentStudent { ParentId = p.ParentId, StudentId = s.StudentId }); }
            await _db.SaveChangesAsync(); return ApiResponse<bool>.Ok(true, "Updated.");
        }
        public async Task<ApiResponse<bool>> DeleteAsync(int userId)
        {
            var p = await _db.Parents.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == userId);
            if (p is null)
                return ApiResponse<bool>.Fail("Not found.");
            p.User.IsActive = false;
            p.User.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Marked inactive.");
        }

        public async Task<ApiResponse<bool>> ToggleActiveAsync(int userId)
        {
            var p = await _db.Parents.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == userId);
            if (p is null)
                return ApiResponse<bool>.Fail("Not found.");
            p.User.IsActive = !p.User.IsActive;
            p.User.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, p.User.IsActive ? "Activated." : "Deactivated.");
        }

        public async Task<ApiResponse<CreatedUserResultDto>> ResetPasswordAsync(int userId)
        {
            var u = await _db.Users.FindAsync(userId);
            if (u is null) return ApiResponse<CreatedUserResultDto>.Fail("Parent not found.");
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
                Role = "Parent"
            }, "Password reset successfully.");
        }
    }
}
