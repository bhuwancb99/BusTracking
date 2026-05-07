using BusTracking.Common.Interfaces;
using System.Security.Cryptography;

namespace BusTracking.Common.Services
{
    public class PasswordService : IPasswordService
    {
        public (string hash, string salt) HashPassword(string plainText)
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
            var hash = BCrypt.Net.BCrypt.HashPassword(plainText, salt);
            return (hash, salt);
        }

        public bool VerifyPassword(string plainText, string hash, string salt)
            => BCrypt.Net.BCrypt.Verify(plainText, hash);

        public string GenerateRandomPassword(int length = 10)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$";
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }
    }
}
