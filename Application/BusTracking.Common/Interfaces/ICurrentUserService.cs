namespace BusTracking.Common.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        int? SchoolId { get; }
        string? UserRole { get; }
    }
}
