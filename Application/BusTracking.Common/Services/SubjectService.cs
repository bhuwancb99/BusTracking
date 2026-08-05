namespace BusTracking.Common.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public SubjectService(AppDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<PagedResult<SubjectDto>>> GetAllAsync(string? search, bool? isActive, int page = 1, int pageSize = 10)
        {
            var schoolId = _currentUser.SchoolId;
            var query = _db.Subjects.AsNoTracking().AsQueryable();

            if (schoolId.HasValue)
            {
                query = query.Where(s => s.SchoolId == schoolId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.SubjectName.Contains(search) || (s.SubjectCode != null && s.SubjectCode.Contains(search)));
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            page = PaginationHelper.Clamp(page);
            var total = await query.CountAsync();

            var items = await query
                .OrderBy(s => s.SubjectName)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(s => new SubjectDto
                {
                    SubjectId = s.SubjectId,
                    SubjectName = s.SubjectName,
                    SubjectCode = s.SubjectCode,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt
                }).ToListAsync();

            return ApiResponse<PagedResult<SubjectDto>>.Ok(new PagedResult<SubjectDto>
            {
                Items = items,
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize
            });
        }

        public async Task<ApiResponse<List<SubjectDto>>> GetActiveSubjectsAsync()
        {
            var schoolId = _currentUser.SchoolId;
            var query = _db.Subjects.AsNoTracking().Where(s => s.IsActive);

            if (schoolId.HasValue)
            {
                query = query.Where(s => s.SchoolId == schoolId.Value);
            }

            var items = await query
                .OrderBy(s => s.SubjectName)
                .Select(s => new SubjectDto
                {
                    SubjectId = s.SubjectId,
                    SubjectName = s.SubjectName,
                    SubjectCode = s.SubjectCode,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt
                }).ToListAsync();

            return ApiResponse<List<SubjectDto>>.Ok(items);
        }

        public async Task<ApiResponse<SubjectDto>> GetByIdAsync(int subjectId)
        {
            var s = await _db.Subjects.FindAsync(subjectId);
            if (s is null)
                return ApiResponse<SubjectDto>.Fail("Subject not found.");

            return ApiResponse<SubjectDto>.Ok(new SubjectDto
            {
                SubjectId = s.SubjectId,
                SubjectName = s.SubjectName,
                SubjectCode = s.SubjectCode,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            });
        }

        public async Task<ApiResponse<SubjectDto>> CreateAsync(CreateSubjectDto dto)
        {
            var schoolId = _currentUser.SchoolId ?? 1;
            var name = dto.SubjectName.Trim();

            var exists = await _db.Subjects.AnyAsync(s => s.SubjectName == name);
            if (exists)
                return ApiResponse<SubjectDto>.Fail($"Subject '{name}' already exists.");

            var entity = new SubjectMaster
            {
                SchoolId = schoolId,
                SubjectName = name,
                SubjectCode = dto.SubjectCode?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Subjects.Add(entity);
            await _db.SaveChangesAsync();

            return ApiResponse<SubjectDto>.Ok(new SubjectDto
            {
                SubjectId = entity.SubjectId,
                SubjectName = entity.SubjectName,
                SubjectCode = entity.SubjectCode,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt
            }, "Subject created successfully.");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int subjectId, UpdateSubjectDto dto)
        {
            var s = await _db.Subjects.FindAsync(subjectId);
            if (s is null)
                return ApiResponse<bool>.Fail("Subject not found.");

            var name = dto.SubjectName.Trim();
            if (s.SubjectName != name)
            {
                var exists = await _db.Subjects.AnyAsync(x => x.SubjectName == name && x.SubjectId != subjectId);
                if (exists)
                    return ApiResponse<bool>.Fail($"Subject '{name}' already exists.");

                s.SubjectName = name;
            }

            s.SubjectCode = dto.SubjectCode?.Trim();
            s.IsActive = dto.IsActive;
            s.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Subject updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int subjectId)
        {
            var s = await _db.Subjects.FindAsync(subjectId);
            if (s is null)
                return ApiResponse<bool>.Fail("Subject not found.");

            _db.Subjects.Remove(s);
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Subject deleted successfully.");
        }

        public async Task<ApiResponse<bool>> ToggleActiveAsync(int subjectId)
        {
            var sub = await _db.Subjects.FindAsync(subjectId);
            if (sub is null)
                return ApiResponse<bool>.Fail("Subject not found.");

            sub.IsActive = !sub.IsActive;
            sub.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(sub.IsActive, $"Subject status updated to {(sub.IsActive ? "Active" : "Inactive")}.");
        }
    }
}
