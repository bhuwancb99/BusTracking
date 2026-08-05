namespace BusTracking.Common.Services
{
    public class SectionService : ISectionService
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public SectionService(AppDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<List<SectionDto>>> GetSectionsByStandardAsync(int standardId)
        {
            var schoolId = _currentUser.SchoolId;
            var query = _db.Sections.AsNoTracking().Where(s => s.StandardId == standardId);

            if (schoolId.HasValue)
            {
                query = query.Where(s => s.SchoolId == schoolId.Value);
            }

            var sections = await query
                .OrderBy(s => s.SectionName)
                .Select(s => new SectionDto
                {
                    SectionId = s.SectionId,
                    StandardId = s.StandardId,
                    StandardName = s.Standard.StandardName,
                    SectionName = s.SectionName,
                    IsDefault = s.IsDefault,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt
                }).ToListAsync();

            if (!sections.Any())
            {
                // Trigger auto section 'A' rule if standard exists but has no section
                var defaultSection = await EnsureDefaultSectionAAsync(standardId, schoolId ?? 1);
                if (defaultSection.Success && defaultSection.Data != null)
                {
                    sections.Add(defaultSection.Data);
                }
            }

            return ApiResponse<List<SectionDto>>.Ok(sections);
        }

        public async Task<ApiResponse<SectionDto>> GetByIdAsync(int sectionId)
        {
            var s = await _db.Sections.Include(x => x.Standard).FirstOrDefaultAsync(x => x.SectionId == sectionId);
            if (s is null)
                return ApiResponse<SectionDto>.Fail("Section not found.");

            return ApiResponse<SectionDto>.Ok(new SectionDto
            {
                SectionId = s.SectionId,
                StandardId = s.StandardId,
                StandardName = s.Standard?.StandardName ?? string.Empty,
                SectionName = s.SectionName,
                IsDefault = s.IsDefault,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            });
        }

        public async Task<ApiResponse<SectionDto>> CreateAsync(CreateSectionDto dto)
        {
            var schoolId = _currentUser.SchoolId ?? 1;
            var standard = await _db.StandardMasters.FindAsync(dto.StandardId);
            if (standard is null)
                return ApiResponse<SectionDto>.Fail("Standard not found.");

            var sectionName = string.IsNullOrWhiteSpace(dto.SectionName) ? "A" : dto.SectionName.Trim().ToUpper();

            var exists = await _db.Sections.AnyAsync(s => s.StandardId == dto.StandardId && s.SectionName == sectionName);
            if (exists)
                return ApiResponse<SectionDto>.Fail($"Section '{sectionName}' already exists for this standard.");

            var entity = new Section
            {
                SchoolId = schoolId,
                StandardId = dto.StandardId,
                SectionName = sectionName,
                IsDefault = dto.IsDefault || sectionName == "A",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Sections.Add(entity);
            await _db.SaveChangesAsync();

            return ApiResponse<SectionDto>.Ok(new SectionDto
            {
                SectionId = entity.SectionId,
                StandardId = entity.StandardId,
                StandardName = standard.StandardName,
                SectionName = entity.SectionName,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt
            }, "Section created successfully.");
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int sectionId, UpdateSectionDto dto)
        {
            var s = await _db.Sections.FindAsync(sectionId);
            if (s is null)
                return ApiResponse<bool>.Fail("Section not found.");

            var newName = dto.SectionName.Trim().ToUpper();
            if (s.SectionName != newName)
            {
                var exists = await _db.Sections.AnyAsync(x => x.StandardId == s.StandardId && x.SectionName == newName && x.SectionId != sectionId);
                if (exists)
                    return ApiResponse<bool>.Fail($"Section '{newName}' already exists.");

                s.SectionName = newName;
            }

            s.IsActive = dto.IsActive;
            s.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Section updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int sectionId)
        {
            var s = await _db.Sections.FindAsync(sectionId);
            if (s is null)
                return ApiResponse<bool>.Fail("Section not found.");

            if (s.IsDefault && s.SectionName == "A")
            {
                var hasOtherSections = await _db.Sections.AnyAsync(x => x.StandardId == s.StandardId && x.SectionId != sectionId);
                if (!hasOtherSections)
                {
                    return ApiResponse<bool>.Fail("Cannot delete default Section 'A' when no other sections exist.");
                }
            }

            _db.Sections.Remove(s);
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Section deleted successfully.");
        }

        public async Task<ApiResponse<bool>> ToggleActiveAsync(int sectionId)
        {
            var s = await _db.Sections.FindAsync(sectionId);
            if (s is null)
                return ApiResponse<bool>.Fail("Section not found.");

            s.IsActive = !s.IsActive;
            s.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(s.IsActive, $"Section status updated to {(s.IsActive ? "Active" : "Inactive")}.");
        }

        public async Task<ApiResponse<SectionDto>> EnsureDefaultSectionAAsync(int standardId, int schoolId)
        {
            var standard = await _db.StandardMasters.FindAsync(standardId);
            if (standard is null)
                return ApiResponse<SectionDto>.Fail("Standard not found.");

            var existingA = await _db.Sections.FirstOrDefaultAsync(s => s.StandardId == standardId && s.SectionName == "A");
            if (existingA != null)
            {
                return ApiResponse<SectionDto>.Ok(new SectionDto
                {
                    SectionId = existingA.SectionId,
                    StandardId = existingA.StandardId,
                    StandardName = standard.StandardName,
                    SectionName = existingA.SectionName,
                    IsDefault = existingA.IsDefault,
                    IsActive = existingA.IsActive,
                    CreatedAt = existingA.CreatedAt
                });
            }

            var sectionA = new Section
            {
                SchoolId = schoolId,
                StandardId = standardId,
                SectionName = "A",
                IsDefault = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Sections.Add(sectionA);
            await _db.SaveChangesAsync();

            return ApiResponse<SectionDto>.Ok(new SectionDto
            {
                SectionId = sectionA.SectionId,
                StandardId = sectionA.StandardId,
                StandardName = standard.StandardName,
                SectionName = sectionA.SectionName,
                IsDefault = sectionA.IsDefault,
                IsActive = sectionA.IsActive,
                CreatedAt = sectionA.CreatedAt
            });
        }
    }
}
