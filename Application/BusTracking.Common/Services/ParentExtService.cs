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
                UserName = p.User.UserName,
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
            if (await _db.Users.AnyAsync(u => u.UserName == dto.UserName))
                return ApiResponse<CreatedUserResultDto>.Fail("Username already in use.");
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return ApiResponse<CreatedUserResultDto>.Fail("Email already in use.");

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
            if (dto.SendWelcomeEmail && !string.IsNullOrWhiteSpace(dto.Email))
            {
                try
                {
                    await _email.SendAsync(dto.Email!, "Your Parent Account",
                        $"<p>Hi {dto.FullName},</p><p>Username: <b>{dto.UserName}</b><br>Password: <b>{password}</b></p>");
                    emailSent = true;
                }
                catch { }
            }

            return ApiResponse<CreatedUserResultDto>.Ok(new CreatedUserResultDto
            {
                UserId = user.UserId,
                FullName = dto.FullName,
                UserName = dto.UserName,
                Email = dto.Email,
                GeneratedPassword = password,
                PlainPassword = password,
                EmailSent = emailSent
            }, "Parent created.");
        }

        public async Task<ApiResponse<bool>> UpdateExtAsync(int userId, UpdateParentExtDto dto)
        {
            var p = await _db.Parents.Include(x => x.User).Include(x => x.ParentStudents)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            if (p is null) return ApiResponse<bool>.Fail("Parent not found.");

            if (await _db.Users.AnyAsync(u => u.UserName == dto.UserName && u.UserId != userId))
                return ApiResponse<bool>.Fail("Username already in use.");
            if (!string.IsNullOrWhiteSpace(dto.Email) && await _db.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != userId))
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
                p.User.PasswordHash = hash; p.User.PasswordSalt = salt;
            }

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
