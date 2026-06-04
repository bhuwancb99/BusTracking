namespace BusTracking.Common.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, string role);
        string GenerateToken(int userId, string email, string role, IEnumerable<string> permissions);
        (int userId, string email, string role)? ValidateToken(string token);
    }
}
