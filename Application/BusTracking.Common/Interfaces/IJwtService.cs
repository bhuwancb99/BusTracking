namespace BusTracking.Common.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, string role);
        (int userId, string email, string role)? ValidateToken(string token);
    }
}
