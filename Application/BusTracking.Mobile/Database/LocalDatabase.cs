using BusTracking.Mobile.Models.Auth;
using SQLite;
using System.Security.Cryptography;
using System.Text;

namespace BusTracking.Mobile.Database;

/// <summary>
/// Provides an asynchronous local SQLite database for persisting a single encrypted SessionUser.
/// </summary>
/// <remarks>Initializes an SQLiteAsyncConnection using FileSystem.AppDataDirectory and
/// Constants.Database.Filename and synchronously waits for InitAsync via GetAwaiter().GetResult(), which blocks the
/// calling thread and may propagate initialization exceptions. Persists session tokens encrypted with AES using a key
/// derived from Constants.Database.EncryptionKey (SHA-256); the IV is prepended to the ciphertext and encryption is not
/// authenticated. Treat the encryption key as sensitive and avoid logging or exposing it.</remarks>
public class LocalDatabase
{
   /// <summary>
   /// Asynchronous SQLite connection used for executing queries and transactions.
   /// </summary>
   /// <remarks>Initialized once and reused for asynchronous database operations.</remarks>
    private readonly SQLiteAsyncConnection _db;

    /// <summary>
    /// Database encryption key used for encrypting stored data.
    /// </summary>
    /// <remarks>Retrieved from Constants.Database.EncryptionKey. Treat as sensitive and avoid logging or
    /// exposing it.</remarks>
    private readonly string _encKey = Constants.Database.EncryptionKey;

   /// <summary>
   /// Initializes a new instance of LocalDatabase and creates an asynchronous SQLite connection using the application's
   /// data directory.
   /// </summary>
   /// <remarks>Builds the database path from FileSystem.AppDataDirectory and Constants.Database.Filename,
   /// assigns the SQLiteAsyncConnection to the backing field, and synchronously waits for InitAsync via
   /// GetAwaiter().GetResult(), which blocks the calling thread and may propagate initialization exceptions.</remarks>
    public LocalDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, Constants.Database.Filename);
        _db = new SQLiteAsyncConnection(dbPath);
        InitAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Initializes the database schema by creating the SessionUser table.
    /// </summary>
    /// <remarks>Creates the SessionUser table if it does not exist using the database CreateTableAsync
    /// operation.</remarks>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    private async Task InitAsync()
    {
        await _db.CreateTableAsync<SessionUser>();
    }


    /// <summary>
    /// Encrypts the user's token and saves the session to the database, replacing any existing session.
    /// </summary>
    /// <remarks>Encrypts the token, deletes any existing SessionUser so only one session is stored, and
    /// inserts the provided user. Database and encryption errors may be propagated.</remarks>
    /// <param name="user">Session user whose token will be encrypted and persisted.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveSessionAsync(SessionUser user)
    {
        user.Token = Encrypt(user.Token);
        await _db.DeleteAllAsync<SessionUser>();      // only one session at a time
        await _db.InsertAsync(user);
    }

   /// <summary>
   /// Retrieves the current session user from the database, validates expiry, and decrypts the session token.
   /// </summary>
   /// <remarks>Expired sessions are cleared via ClearSessionAsync and expiry is compared against
   /// DateTime.UtcNow. The session token is decrypted before the user is returned.</remarks>
   /// <returns>A task that returns the session user if one exists and is not expired; otherwise, null.</returns>
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

    /// <summary>
    /// Determines whether a stored session user exists and has not expired.
    /// </summary>
    /// <remarks>Performs an asynchronous database query to retrieve the session user and compares the user's
    /// Expiry against DateTime.UtcNow.</remarks>
    /// <returns>true if a session user exists and DateTime.UtcNow is earlier than the user's Expiry; otherwise, false.</returns>
    public async Task<bool> HasValidSessionAsync()
    {
        var user = await _db.Table<SessionUser>().FirstOrDefaultAsync();
        return user != null && DateTime.UtcNow < user.Expiry;
    }

    /// <summary>
    /// Updates the stored session token and expiry for the first session user in the database asynchronously.
    /// </summary>
    /// <remarks>The token is encrypted before persisting. If no session user exists, the operation completes
    /// without making changes.</remarks>
    /// <param name="newToken">New token to store; it is encrypted before persisting.</param>
    /// <param name="newExpiry">New expiration time for the token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task UpdateTokenAsync(string newToken, DateTime newExpiry)
    {
        var user = await _db.Table<SessionUser>().FirstOrDefaultAsync();
        if (user is null) return;
        user.Token = Encrypt(newToken);
        user.Expiry = newExpiry;
        await _db.UpdateAsync(user);
    }

    /// <summary>
    /// Clears all SessionUser records from the underlying database asynchronously. 
    /// </summary>
    /// <remarks>Exceptions from the database operation propagate to the returned task.</remarks>
    /// <returns>A task that completes when all SessionUser records have been deleted.</returns>
    public async Task ClearSessionAsync()
    {
        await _db.DeleteAllAsync<SessionUser>();
    }


   /// <summary>
   /// Encrypts the provided plaintext using AES with a key derived from the instance field _encKey and returns a
   /// Base64-encoded result that includes the IV followed by the ciphertext.
   /// </summary>
   /// <remarks>Derives the AES key by hashing _encKey with SHA-256 and generates a random IV per call, which
   /// is prepended to the ciphertext for decryption. The implementation does not authenticate ciphertext; for stronger
   /// security, use a dedicated KDF (for example, Rfc2898DeriveBytes) and an authenticated encryption mode (for
   /// example, AES-GCM) or an encrypt-then-MAC scheme.</remarks>
   /// <param name="plainText">Plaintext to encrypt.</param>
   /// <returns>Base64-encoded string containing the IV followed by the ciphertext.</returns>
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

    /// <summary>
    /// Decrypts a base64-encoded AES payload whose first 16 bytes are the IV and returns the UTF-8 plaintext.
    /// </summary>
    /// <remarks>Derives the AES key by computing SHA-256 over the instance field _encKey. Uses the first 16
    /// bytes of the decoded payload as the IV. All exceptions are caught and result in an empty string instead of
    /// throwing.</remarks>
    /// <param name="cipherText">Base64-encoded AES payload with a 16-byte IV prefixed to the ciphertext.</param>
    /// <returns>The decrypted UTF-8 string, or an empty string if decryption fails.</returns>
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
