using BusTracking.Mobile.Models.Auth;
using SQLite;
using System.Security.Cryptography;
using System.Text;

namespace BusTracking.Mobile.Database;

/// <summary>
/// Encrypted local SQLite database.
/// Stores session user (token, role, permissions) securely.
/// Token is AES-encrypted at rest; decrypted on read.
/// </summary>
public class LocalDatabase
{
    private readonly SQLiteAsyncConnection _db;
    private readonly string _encKey = Constants.Database.EncryptionKey;

    public LocalDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, Constants.Database.Filename);
        _db = new SQLiteAsyncConnection(dbPath);
        InitAsync().GetAwaiter().GetResult();
    }

    private async Task InitAsync()
    {
        await _db.CreateTableAsync<SessionUser>();
    }

    // ── Session / User ────────────────────────────────────────────────────

    /// <summary>Save user session after successful login. Token is encrypted.</summary>
    public async Task SaveSessionAsync(SessionUser user)
    {
        user.Token = Encrypt(user.Token);
        await _db.DeleteAllAsync<SessionUser>();      // only one session at a time
        await _db.InsertAsync(user);
    }

    /// <summary>Load session and decrypt token. Returns null if no session or token expired.</summary>
    public async Task<SessionUser?> GetSessionAsync()
    {
        var user = await _db.Table<SessionUser>().FirstOrDefaultAsync();
        if (user is null) return null;

        // Check expiry
        if (DateTime.UtcNow >= user.Expiry)
        {
            await ClearSessionAsync();
            return null;
        }

        user.Token = Decrypt(user.Token);
        return user;
    }

    /// <summary>Check if a valid (non-expired) session exists.</summary>
    public async Task<bool> HasValidSessionAsync()
    {
        var user = await _db.Table<SessionUser>().FirstOrDefaultAsync();
        return user != null && DateTime.UtcNow < user.Expiry;
    }

    /// <summary>Update the token and expiry (after token refresh).</summary>
    public async Task UpdateTokenAsync(string newToken, DateTime newExpiry)
    {
        var user = await _db.Table<SessionUser>().FirstOrDefaultAsync();
        if (user is null) return;
        user.Token = Encrypt(newToken);
        user.Expiry = newExpiry;
        await _db.UpdateAsync(user);
    }

    /// <summary>Clear session (logout).</summary>
    public async Task ClearSessionAsync()
    {
        await _db.DeleteAllAsync<SessionUser>();
    }

    // ── AES Encryption ────────────────────────────────────────────────────

    private string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        var key = SHA256.HashData(Encoding.UTF8.GetBytes(_encKey));
        aes.Key = key;
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        // Prepend IV to cipher for decryption
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        aes.IV.CopyTo(result, 0);
        cipherBytes.CopyTo(result, aes.IV.Length);
        return Convert.ToBase64String(result);
    }

    private string Decrypt(string cipherText)
    {
        try
        {
            var fullBytes = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            var key = SHA256.HashData(Encoding.UTF8.GetBytes(_encKey));
            aes.Key = key;
            var iv = fullBytes[..16];
            var cipher = fullBytes[16..];
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(plain);
        }
        catch
        {
            return "";
        }
    }
}