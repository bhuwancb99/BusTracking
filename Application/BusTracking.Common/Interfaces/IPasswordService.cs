namespace BusTracking.Common.Interfaces
{
    public interface IPasswordService
    {
        (string hash, string salt) HashPassword(string plainText);
        bool VerifyPassword(string plainText, string hash, string salt);
        string GenerateRandomPassword(int length = 10);
    }
}
