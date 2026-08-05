namespace BusTracking.Common.Services
{
    public class ClassMappingService : IClassMappingService
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public ClassMappingService(AppDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<List<ClassSubjectTeacherDto>>> GetClassMappingsAsync(int academicYearId, int standardId, int? sectionId)
        {
            var schoolId = _currentUser.SchoolId;
            var query = _db.ClassSubjectTeachers.AsNoTracking()
                .Include(c => c.AcademicYear)
                .Include(c => c.Standard)
                .Include(c => c.Section)
                .Include(c => c.Subject)
                .Include(c => c.Teacher).ThenInclude(t => t.User)
                .Where(c => c.AcademicYearId == academicYearId && c.StandardId == standardId);

            if (schoolId.HasValue)
            {
                query = query.Where(c => c.SchoolId == schoolId.Value);
            }

            if (sectionId.HasValue && sectionId.Value > 0)
            {
                query = query.Where(c => c.SectionId == sectionId.Value);
            }

            var items = await query.Select(c => new ClassSubjectTeacherDto
            {
                ClassSubjectTeacherId = c.ClassSubjectTeacherId,
                AcademicYearId = c.AcademicYearId,
                YearName = c.AcademicYear.YearName,
                StandardId = c.StandardId,
                StandardName = c.Standard.StandardName,
                SectionId = c.SectionId,
                SectionName = c.Section.SectionName,
                SubjectId = c.SubjectId,
                SubjectName = c.Subject.SubjectName,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher.User.FullName,
                IsActive = c.IsActive
            }).ToListAsync();

            return ApiResponse<List<ClassSubjectTeacherDto>>.Ok(items);
        }

        public async Task<ApiResponse<ClassSubjectTeacherDto>> AssignSubjectTeacherAsync(AssignClassSubjectTeacherDto dto)
        {
            var schoolId = _currentUser.SchoolId ?? 1;

            var existing = await _db.ClassSubjectTeachers.FirstOrDefaultAsync(c =>
                c.AcademicYearId == dto.AcademicYearId &&
                c.StandardId == dto.StandardId &&
                c.SectionId == dto.SectionId &&
                c.SubjectId == dto.SubjectId);

            if (existing != null)
            {
                existing.TeacherId = dto.TeacherId;
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                existing = new ClassSubjectTeacher
                {
                    SchoolId = schoolId,
                    AcademicYearId = dto.AcademicYearId,
                    StandardId = dto.StandardId,
                    SectionId = dto.SectionId,
                    SubjectId = dto.SubjectId,
                    TeacherId = dto.TeacherId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.ClassSubjectTeachers.Add(existing);
            }

            await _db.SaveChangesAsync();

            var loaded = await _db.ClassSubjectTeachers.AsNoTracking()
                .Include(c => c.AcademicYear)
                .Include(c => c.Standard)
                .Include(c => c.Section)
                .Include(c => c.Subject)
                .Include(c => c.Teacher).ThenInclude(t => t.User)
                .FirstAsync(c => c.ClassSubjectTeacherId == existing.ClassSubjectTeacherId);

            return ApiResponse<ClassSubjectTeacherDto>.Ok(new ClassSubjectTeacherDto
            {
                ClassSubjectTeacherId = loaded.ClassSubjectTeacherId,
                AcademicYearId = loaded.AcademicYearId,
                YearName = loaded.AcademicYear.YearName,
                StandardId = loaded.StandardId,
                StandardName = loaded.Standard.StandardName,
                SectionId = loaded.SectionId,
                SectionName = loaded.Section.SectionName,
                SubjectId = loaded.SubjectId,
                SubjectName = loaded.Subject.SubjectName,
                TeacherId = loaded.TeacherId,
                TeacherName = loaded.Teacher.User.FullName,
                IsActive = loaded.IsActive
            }, "Teacher assigned to subject successfully.");
        }

        public async Task<ApiResponse<ClassSubjectTeacherDto>> GetByIdAsync(int id)
        {
            var loaded = await _db.ClassSubjectTeachers.AsNoTracking()
                .Include(c => c.AcademicYear)
                .Include(c => c.Standard)
                .Include(c => c.Section)
                .Include(c => c.Subject)
                .Include(c => c.Teacher).ThenInclude(t => t.User)
                .FirstOrDefaultAsync(c => c.ClassSubjectTeacherId == id);

            if (loaded == null) return ApiResponse<ClassSubjectTeacherDto>.Fail("Mapping not found.");

            return ApiResponse<ClassSubjectTeacherDto>.Ok(new ClassSubjectTeacherDto
            {
                ClassSubjectTeacherId = loaded.ClassSubjectTeacherId,
                AcademicYearId = loaded.AcademicYearId,
                YearName = loaded.AcademicYear.YearName,
                StandardId = loaded.StandardId,
                StandardName = loaded.Standard.StandardName,
                SectionId = loaded.SectionId,
                SectionName = loaded.Section.SectionName,
                SubjectId = loaded.SubjectId,
                SubjectName = loaded.Subject.SubjectName,
                TeacherId = loaded.TeacherId,
                TeacherName = loaded.Teacher.User.FullName,
                IsActive = loaded.IsActive
            });
        }

        public async Task<ApiResponse<ClassSubjectTeacherDto>> UpdateSubjectTeacherAsync(int id, AssignClassSubjectTeacherDto dto)
        {
            var item = await _db.ClassSubjectTeachers.FindAsync(id);
            if (item is null)
                return ApiResponse<ClassSubjectTeacherDto>.Fail("Mapping not found.");

            var existingOther = await _db.ClassSubjectTeachers.FirstOrDefaultAsync(c =>
                c.ClassSubjectTeacherId != id &&
                c.AcademicYearId == dto.AcademicYearId &&
                c.StandardId == dto.StandardId &&
                c.SectionId == dto.SectionId &&
                c.SubjectId == dto.SubjectId);

            if (existingOther != null)
            {
                return ApiResponse<ClassSubjectTeacherDto>.Fail("Mapping already exists for this subject, class, and section.");
            }

            item.AcademicYearId = dto.AcademicYearId;
            item.StandardId = dto.StandardId;
            item.SectionId = dto.SectionId;
            item.SubjectId = dto.SubjectId;
            item.TeacherId = dto.TeacherId;
            item.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var loaded = await _db.ClassSubjectTeachers.AsNoTracking()
                .Include(c => c.AcademicYear)
                .Include(c => c.Standard)
                .Include(c => c.Section)
                .Include(c => c.Subject)
                .Include(c => c.Teacher).ThenInclude(t => t.User)
                .FirstAsync(c => c.ClassSubjectTeacherId == item.ClassSubjectTeacherId);

            return ApiResponse<ClassSubjectTeacherDto>.Ok(new ClassSubjectTeacherDto
            {
                ClassSubjectTeacherId = loaded.ClassSubjectTeacherId,
                AcademicYearId = loaded.AcademicYearId,
                YearName = loaded.AcademicYear.YearName,
                StandardId = loaded.StandardId,
                StandardName = loaded.Standard.StandardName,
                SectionId = loaded.SectionId,
                SectionName = loaded.Section.SectionName,
                SubjectId = loaded.SubjectId,
                SubjectName = loaded.Subject.SubjectName,
                TeacherId = loaded.TeacherId,
                TeacherName = loaded.Teacher.User.FullName,
                IsActive = loaded.IsActive
            }, "Mapping updated successfully.");
        }

        public async Task<ApiResponse<bool>> UnassignSubjectTeacherAsync(int classSubjectTeacherId)
        {
            var item = await _db.ClassSubjectTeachers.FindAsync(classSubjectTeacherId);
            if (item is null)
                return ApiResponse<bool>.Fail("Mapping not found.");

            _db.ClassSubjectTeachers.Remove(item);
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Mapping removed successfully.");
        }
    }
}
