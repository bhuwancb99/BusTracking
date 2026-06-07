namespace BusTracking.Common.Interfaces;

public interface IImageService
{
    /// <summary>
    /// Save a profile image for any user role.
    /// Deletes old file first if existingUrl is provided.
    /// Returns the new public URL  e.g. /images/student/u_88.jpg
    /// </summary>
    Task<string> SaveProfileImageAsync(
        IFormFile file,
        int userId,
        string role,          // "superadmin" | "coordinator" | "driver" | "student" | "parent"
        string? existingUrl);

    /// <summary>
    /// Save one bus image. Returns public URL e.g. /images/bus/b_12_3.jpg
    /// </summary>
    Task<string> SaveBusImageAsync(IFormFile file, int busId, int imageIndex);

    /// <summary>Delete a file by its public URL. Silent on missing file.</summary>
    void DeleteFile(string? publicUrl);
}
