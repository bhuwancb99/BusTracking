namespace BusTracking.Common.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly AppDbContext _db;
        private readonly IPasswordService _pwd;

        public TeacherService(AppDbContext db, IPasswordService pwd)
        {
            _db = db;
            _pwd = pwd;
        }

        public async Task<PagedResult<TeacherDto>> GetTeachersAsync(int? schoolId, string? search, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _db.Teachers
                .Include(t => t.User)
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AsQueryable();

            if (schoolId.HasValue && schoolId.Value > 0)
            {
                query = query.Where(t => t.SchoolId == schoolId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(t =>
                    t.User.FullName.Contains(s) ||
                    t.User.UserName.Contains(s) ||
                    (t.User.Email != null && t.User.Email.Contains(s)) ||
                    (t.EmployeeCode != null && t.EmployeeCode.Contains(s)) ||
                    (t.Department != null && t.Department.Contains(s)) ||
                    (t.Designation != null && t.Designation.Contains(s))
                );
            }

            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TeacherDto
                {
                    TeacherId = t.TeacherId,
                    UserId = t.UserId,
                    SchoolId = t.SchoolId,
                    FullName = t.User.FullName,
                    UserName = t.User.UserName,
                    Email = t.User.Email,
                    PhoneNumber = t.User.PhoneNumber,
                    ProfileImageUrl = t.User.ProfileImageUrl,
                    EmployeeCode = t.EmployeeCode,
                    Qualification = t.Qualification,
                    Designation = t.Designation,
                    Department = t.Department,
                    JoiningDate = t.JoiningDate,
                    Gender = t.Gender,
                    EmergencyContact = t.EmergencyContact,
                    IsActive = t.User.IsActive,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return new PagedResult<TeacherDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResponse<TeacherDto>> GetTeacherByIdAsync(int teacherId)
        {
            var t = await _db.Teachers
                .Include(x => x.User)
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TeacherId == teacherId);

            if (t == null) return ApiResponse<TeacherDto>.Fail("Teacher record not found.");

            return ApiResponse<TeacherDto>.Ok(MapToDto(t));
        }

        public async Task<ApiResponse<TeacherDto>> GetTeacherByUserIdAsync(int userId)
        {
            var t = await _db.Teachers
                .Include(x => x.User)
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (t == null) return ApiResponse<TeacherDto>.Fail("Teacher profile not found.");

            return ApiResponse<TeacherDto>.Ok(MapToDto(t));
        }

        public async Task<ApiResponse<TeacherDto>> CreateTeacherAsync(CreateTeacherDto dto, string? profileImageUrl = null)
        {
            // Validate unique Username
            var usernameExists = await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.UserName == dto.UserName.Trim());
            if (usernameExists)
                return ApiResponse<TeacherDto>.Fail("Username already exists in the system.");

            // Create User account (RoleId = 3 for Teacher)
            var (hash, salt) = _pwd.HashPassword(dto.Password);
            var user = new User
            {
                SchoolId = dto.SchoolId,
                RoleId = 3, // RoleId = 3 is Teacher
                FullName = dto.FullName.Trim(),
                UserName = dto.UserName.Trim(),
                Email = dto.Email?.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                ProfileImageUrl = profileImageUrl,
                IsActive = dto.IsActive,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Create Teacher detail record
            var teacher = new Teacher
            {
                UserId = user.UserId,
                SchoolId = dto.SchoolId,
                EmployeeCode = dto.EmployeeCode?.Trim(),
                Qualification = dto.Qualification?.Trim(),
                Designation = dto.Designation?.Trim(),
                Department = dto.Department?.Trim(),
                JoiningDate = dto.JoiningDate,
                Gender = dto.Gender?.Trim(),
                EmergencyContact = dto.EmergencyContact?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Teachers.Add(teacher);
            await _db.SaveChangesAsync();

            teacher.User = user;
            return ApiResponse<TeacherDto>.Ok(MapToDto(teacher), "Teacher account created successfully.");
        }

        public async Task<ApiResponse<TeacherDto>> UpdateTeacherAsync(UpdateTeacherDto dto, string? profileImageUrl = null)
        {
            var teacher = await _db.Teachers
                .Include(t => t.User)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.TeacherId == dto.TeacherId);

            if (teacher == null) return ApiResponse<TeacherDto>.Fail("Teacher record not found.");

            // Validate unique Username if changed
            var usernameExists = await _db.Users.IgnoreQueryFilters()
                .AnyAsync(u => u.UserName == dto.UserName.Trim() && u.UserId != teacher.UserId);
            if (usernameExists)
                return ApiResponse<TeacherDto>.Fail("Username already taken by another account.");

            // Update User details
            teacher.User.FullName = dto.FullName.Trim();
            teacher.User.UserName = dto.UserName.Trim();
            teacher.User.Email = dto.Email?.Trim();
            teacher.User.PhoneNumber = dto.PhoneNumber?.Trim();
            teacher.User.IsActive = dto.IsActive;
            teacher.User.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(profileImageUrl))
            {
                teacher.User.ProfileImageUrl = profileImageUrl;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var (hash, salt) = _pwd.HashPassword(dto.Password);
                teacher.User.PasswordHash = hash;
                teacher.User.PasswordSalt = salt;
            }

            // Update Teacher details
            teacher.EmployeeCode = dto.EmployeeCode?.Trim();
            teacher.Qualification = dto.Qualification?.Trim();
            teacher.Designation = dto.Designation?.Trim();
            teacher.Department = dto.Department?.Trim();
            teacher.JoiningDate = dto.JoiningDate;
            teacher.Gender = dto.Gender?.Trim();
            teacher.EmergencyContact = dto.EmergencyContact?.Trim();
            teacher.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ApiResponse<TeacherDto>.Ok(MapToDto(teacher), "Teacher account updated successfully.");
        }

        public async Task<ApiResponse<bool>> ToggleTeacherStatusAsync(int teacherId)
        {
            var teacher = await _db.Teachers
                .Include(t => t.User)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null) return ApiResponse<bool>.Fail("Teacher record not found.");

            teacher.User.IsActive = !teacher.User.IsActive;
            teacher.User.UpdatedAt = DateTime.UtcNow;
            teacher.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(teacher.User.IsActive, $"Teacher status updated to {(teacher.User.IsActive ? "Active" : "Inactive")}.");
        }

        public async Task<ApiResponse<bool>> CheckUsernameAvailabilityAsync(string userName, int? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return ApiResponse<bool>.Fail("Username is required.");

            var exists = await _db.Users.IgnoreQueryFilters()
                .AnyAsync(u => u.UserName == userName.Trim() && (!excludeUserId.HasValue || u.UserId != excludeUserId.Value));

            if (exists)
                return ApiResponse<bool>.Fail("Username is already taken.");

            return ApiResponse<bool>.Ok(true, "Username is available.");
        }

        private static TeacherDto MapToDto(Teacher t)
        {
            return new TeacherDto
            {
                TeacherId = t.TeacherId,
                UserId = t.UserId,
                SchoolId = t.SchoolId,
                FullName = t.User?.FullName ?? "",
                UserName = t.User?.UserName ?? "",
                Email = t.User?.Email,
                PhoneNumber = t.User?.PhoneNumber,
                ProfileImageUrl = t.User?.ProfileImageUrl,
                EmployeeCode = t.EmployeeCode,
                Qualification = t.Qualification,
                Designation = t.Designation,
                Department = t.Department,
                JoiningDate = t.JoiningDate,
                Gender = t.Gender,
                EmergencyContact = t.EmergencyContact,
                IsActive = t.User?.IsActive ?? false,
                CreatedAt = t.CreatedAt
            };
        }
    }
}
