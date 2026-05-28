namespace BusTracking.Mobile.Database;

public class LocalDatabase
{
    private SQLiteAsyncConnection? _db;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly string _encKey = Constants.Database.EncryptionKey;

    // ── Lazy init — called by every method before touching _db ────────────
    private async Task<SQLiteAsyncConnection> GetDbAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (_db is not null) return _db;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, Constants.Database.Filename);
            _db = new SQLiteAsyncConnection(dbPath);
            await _db.CreateTableAsync<SessionUser>();
            return _db;
        }
        finally
        {
            _lock.Release();
        }
    }

    // ── Session ───────────────────────────────────────────────────────────
    public async Task SaveSessionAsync(SessionUser user)
    {
        var db = await GetDbAsync();
        user.Token = Encrypt(user.Token);
        await db.DeleteAllAsync<SessionUser>();      // only one session at a time
        await db.InsertAsync(user);
    }

    public async Task<SessionUser?> GetSessionAsync()
    {
        var db = await GetDbAsync();
        var user = await db.Table<SessionUser>().FirstOrDefaultAsync();
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

    public async Task<bool> HasValidSessionAsync()
    {
        var db = await GetDbAsync();
        var user = await db.Table<SessionUser>().FirstOrDefaultAsync();
        return user != null && DateTime.UtcNow < user.Expiry;
    }

    public async Task UpdateTokenAsync(string newToken, DateTime newExpiry)
    {
        var db = await GetDbAsync();
        var user = await db.Table<SessionUser>().FirstOrDefaultAsync();
        if (user is null) return;
        user.Token = Encrypt(newToken);
        user.Expiry = newExpiry;
        await db.UpdateAsync(user);
    }

    public async Task ClearSessionAsync()
    {
        var db = await GetDbAsync();
        await db.DeleteAllAsync<SessionUser>();
    }

    // ── Encryption ────────────────────────────────────────────────────────
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