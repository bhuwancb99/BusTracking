namespace BusTracking.Common.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, string role, int? schoolId = null);
        string GenerateToken(int userId, string email, string role, IEnumerable<string> permissions, int? schoolId = null);
        (int userId, string email, string role, int? schoolId)? ValidateToken(string token);
    }
}
