namespace BusTracking.Common.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        int? SchoolId { get; }
        string? UserRole { get; }
        string? TimeZoneInfoId { get; }
        DateTime SchoolNow { get; }
        DateOnly SchoolToday { get; }
    }
}
