namespace BusTracking.Common.Services
{
    public class StudentSearchService : IStudentSearchService
    {
        private readonly AppDbContext _db;
        public StudentSearchService(AppDbContext db) => _db = db;

        public async Task<List<StudentSearchDto>> SearchAsync(string query, int maxResults = 10)
        {
            if (string.IsNullOrWhiteSpace(query)) return [];
            query = query.Trim().ToLower();

            return await _db.Students
                .Include(s => s.User)
                .Include(s => s.Standard)
                .Include(s => s.Bus)
                .Where(s => s.User.IsActive
                    && (s.StudentCode.ToLower().Contains(query)
                        || s.User.FullName.ToLower().Contains(query)))
                .OrderBy(s => s.StudentCode)
                .Take(maxResults)
                .Select(s => new StudentSearchDto
                {
                    StudentId = s.StudentId,
                    StudentCode = s.StudentCode,
                    FullName = s.User.FullName,
                    StandardId = s.StandardId,
                    StandardName = s.Standard != null ? s.Standard.StandardName : null,
                    BusNumber = s.Bus != null ? s.Bus.BusNumber : null
                })
                .ToListAsync();
        }
    }
}
