using BusTracking.Common.Data;
using BusTracking.Common.DTOs.Common;
using BusTracking.Common.DTOs.Dashboard;
using BusTracking.Common.Enums;
using BusTracking.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTracking.Common.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;
        public DashboardService(AppDbContext db) => _db = db;

        public async Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync()
        {
            var driverRoleId = await _db.Roles.Where(r => r.RoleName == "Driver").Select(r => r.RoleId).FirstAsync();
            var coordinatorRoleId = await _db.Roles.Where(r => r.RoleName == "BusCoordinator").Select(r => r.RoleId).FirstAsync();

            var dto = new DashboardSummaryDto
            {
                TotalBuses = await _db.Buses.CountAsync(b => b.IsActive),
                TotalDrivers = await _db.Users.CountAsync(u => u.RoleId == driverRoleId && u.IsActive),
                TotalBusCoordinators = await _db.Users.CountAsync(u => u.RoleId == coordinatorRoleId && u.IsActive),
                TotalParents = await _db.Parents.CountAsync(p => p.User.IsActive),
                TotalStudents = await _db.Students.CountAsync(s => s.User.IsActive),
                ActiveTrips = await _db.BusTrips.CountAsync(t => t.Status == TripStatus.InProgress)
            };

            return ApiResponse<DashboardSummaryDto>.Ok(dto);
        }
    }
}
