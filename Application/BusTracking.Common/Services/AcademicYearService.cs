namespace BusTracking.Common.Services;

public class AcademicYearService : IAcademicYearService
{
    private readonly AppDbContext _db;

    public AcademicYearService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AcademicYearDto>> GetAcademicYearsAsync(int schoolId, bool activeOnly = false)
    {
        var query = _db.AcademicYears.AsNoTracking().Where(a => a.SchoolId == schoolId);
        if (activeOnly)
        {
            query = query.Where(a => a.IsActive);
        }

        return await query
            .OrderByDescending(a => a.StartDate)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    public async Task<AcademicYearDto?> GetByIdAsync(int academicYearId)
    {
        var year = await _db.AcademicYears.AsNoTracking().FirstOrDefaultAsync(a => a.AcademicYearId == academicYearId);
        return year != null ? MapToDto(year) : null;
    }

    public async Task<AcademicYearDto?> GetActiveSessionAsync(int schoolId)
    {
        var current = await _db.AcademicYears.AsNoTracking()
            .FirstOrDefaultAsync(a => a.SchoolId == schoolId && a.IsCurrent && a.IsActive);

        if (current is null)
        {
            current = await _db.AcademicYears.AsNoTracking()
                .Where(a => a.SchoolId == schoolId && a.IsActive)
                .OrderByDescending(a => a.StartDate)
                .FirstOrDefaultAsync();
        }

        return current != null ? MapToDto(current) : null;
    }

    public async Task<OperationResult<AcademicYearDto>> CreateAcademicYearAsync(CreateAcademicYearRequest request, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(request.YearName))
            return OperationResult<AcademicYearDto>.Fail("Academic Year Name (e.g. 2025-2026) is required.");

        var exists = await _db.AcademicYears
            .AnyAsync(a => a.SchoolId == request.SchoolId && a.YearName.ToLower() == request.YearName.Trim().ToLower());

        if (exists)
            return OperationResult<AcademicYearDto>.Fail($"Academic Year '{request.YearName}' already exists for this school.");

        if (request.SetAsCurrent)
        {
            var existingCurrent = await _db.AcademicYears
                .Where(a => a.SchoolId == request.SchoolId && a.IsCurrent)
                .ToListAsync();
            foreach (var item in existingCurrent) item.IsCurrent = false;
        }

        var year = new AcademicYear
        {
            SchoolId = request.SchoolId,
            YearName = request.YearName.Trim(),
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            IsActive = request.SetAsCurrent || request.IsActive,
            IsCurrent = request.SetAsCurrent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.AcademicYears.Add(year);
        await _db.SaveChangesAsync();

        return OperationResult<AcademicYearDto>.Ok(MapToDto(year));
    }

    public async Task<OperationResult<AcademicYearDto>> UpdateAcademicYearAsync(UpdateAcademicYearRequest request, string? updatedBy)
    {
        var year = await _db.AcademicYears.FirstOrDefaultAsync(a => a.AcademicYearId == request.AcademicYearId);
        if (year is null)
            return OperationResult<AcademicYearDto>.Fail("Academic Year not found.");

        var exists = await _db.AcademicYears
            .AnyAsync(a => a.SchoolId == year.SchoolId && a.AcademicYearId != request.AcademicYearId && a.YearName.ToLower() == request.YearName.Trim().ToLower());

        if (exists)
            return OperationResult<AcademicYearDto>.Fail($"Another Academic Year with name '{request.YearName}' already exists.");

        if (request.SetAsCurrent && !year.IsCurrent)
        {
            var existingCurrent = await _db.AcademicYears
                .Where(a => a.SchoolId == year.SchoolId && a.AcademicYearId != year.AcademicYearId && a.IsCurrent)
                .ToListAsync();
            foreach (var item in existingCurrent) item.IsCurrent = false;
        }

        year.YearName = request.YearName.Trim();
        year.StartDate = request.StartDate.Date;
        year.EndDate = request.EndDate.Date;
        year.IsActive = request.SetAsCurrent || request.IsActive;
        year.IsCurrent = request.SetAsCurrent;
        year.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return OperationResult<AcademicYearDto>.Ok(MapToDto(year));
    }

    public async Task<OperationResult<bool>> SetActiveAcademicYearAsync(int schoolId, int academicYearId, string? updatedBy)
    {
        var years = await _db.AcademicYears.Where(a => a.SchoolId == schoolId).ToListAsync();
        var target = years.FirstOrDefault(a => a.AcademicYearId == academicYearId);
        if (target is null)
            return OperationResult<bool>.Fail("Selected Academic Year does not exist.");

        if (!target.IsActive)
            return OperationResult<bool>.Fail("Cannot set an inactive Academic Year as current session.");

        foreach (var item in years)
        {
            item.IsCurrent = (item.AcademicYearId == academicYearId);
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return OperationResult<bool>.Ok(true);
    }

    public async Task<OperationResult<bool>> ToggleAcademicYearStatusAsync(int academicYearId, string? updatedBy)
    {
        var year = await _db.AcademicYears.FirstOrDefaultAsync(a => a.AcademicYearId == academicYearId);
        if (year is null)
            return OperationResult<bool>.Fail("Academic Year not found.");

        if (year.IsCurrent && year.IsActive)
        {
            return OperationResult<bool>.Fail("Cannot deactivate the current active academic session. Please activate another session first.");
        }

        year.IsActive = !year.IsActive;
        year.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return OperationResult<bool>.Ok(true, $"Academic Year status updated to {(year.IsActive ? "Active" : "Inactive")}.");
    }

    private static AcademicYearDto MapToDto(AcademicYear a) => new()
    {
        AcademicYearId = a.AcademicYearId,
        SchoolId = a.SchoolId ?? 0,
        YearName = a.YearName,
        StartDate = a.StartDate,
        EndDate = a.EndDate,
        IsActive = a.IsActive,
        IsCurrent = a.IsCurrent,
        CreatedAt = a.CreatedAt
    };
}
