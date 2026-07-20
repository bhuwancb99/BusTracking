namespace BusTracking.Common.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, string role, int? schoolId = null, string? timeZoneInfoId = null);
        string GenerateToken(int userId, string email, string role, IEnumerable<string> permissions, int? schoolId = null, string? timeZoneInfoId = null);
        (int userId, string email, string role, int? schoolId, string? timeZoneInfoId)? ValidateToken(string token);
    }
}
