namespace BusTracking.Common.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public DashboardService(AppDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync()
        {
            var schoolId = _currentUser.SchoolId;
            var driverRoleId = await _db.Roles.Where(r => r.RoleName == "Driver").Select(r => r.RoleId).FirstAsync();
            var coordinatorRoleId = await _db.Roles.Where(r => r.RoleName == "BusCoordinator").Select(r => r.RoleId).FirstAsync();

            var busesQuery = _db.Buses.IgnoreQueryFilters().Where(b => b.IsActive);
            var driversQuery = _db.Users.IgnoreQueryFilters().Where(u => u.RoleId == driverRoleId && u.IsActive);
            var coordinatorsQuery = _db.Users.IgnoreQueryFilters().Where(u => u.RoleId == coordinatorRoleId && u.IsActive);
            var parentsQuery = _db.Parents.IgnoreQueryFilters().Where(p => p.User.IsActive);
            var studentsQuery = _db.Students.IgnoreQueryFilters().Where(s => s.User.IsActive);
            var tripsQuery = _db.BusTrips.IgnoreQueryFilters().Where(t => t.Status == TripStatus.InProgress);

            if (schoolId.HasValue)
            {
                busesQuery = busesQuery.Where(b => b.SchoolId == schoolId.Value);
                driversQuery = driversQuery.Where(u => u.SchoolId == schoolId.Value);
                coordinatorsQuery = coordinatorsQuery.Where(u => u.SchoolId == schoolId.Value);
                parentsQuery = parentsQuery.Where(p => p.SchoolId == schoolId.Value || (p.User != null && p.User.SchoolId == schoolId.Value));
                studentsQuery = studentsQuery.Where(s => s.SchoolId == schoolId.Value || (s.User != null && s.User.SchoolId == schoolId.Value));
                tripsQuery = tripsQuery.Where(t => t.SchoolId == schoolId.Value || (t.Bus != null && t.Bus.SchoolId == schoolId.Value));
            }

            var dto = new DashboardSummaryDto
            {
                TotalBuses = await busesQuery.CountAsync(),
                TotalDrivers = await driversQuery.CountAsync(),
                TotalBusCoordinators = await coordinatorsQuery.CountAsync(),
                TotalParents = await parentsQuery.CountAsync(),
                TotalStudents = await studentsQuery.CountAsync(),
                ActiveTrips = await tripsQuery.CountAsync()
            };

            return ApiResponse<DashboardSummaryDto>.Ok(dto);
        }
    }
}
