namespace BusTracking.Common.Interfaces;

public interface IImageService
{
    /// <summary>
    /// Save or replace a profile image for any user role.
    /// Deletes old file first if existingUrl is provided.
    /// Returns the new full public URL stored in DB.
    /// </summary>
    Task<string> SaveProfileImageAsync(
        IFormFile file, int userId, string role, string? existingUrl);

    /// <summary>
    /// Save one bus image. Returns full public URL stored in DB.
    /// </summary>
    Task<string> SaveBusImageAsync(IFormFile file, int busId, int imageIndex);

    /// <summary>Delete a file by its public URL. Silent on missing file.</summary>
    void DeleteFile(string? publicUrl);

    /// <summary>
    /// Save or replace a school logo. Returns full public URL.
    /// </summary>
    Task<string> SaveSchoolLogoAsync(IFormFile file, int schoolId, string? existingUrl);
}
