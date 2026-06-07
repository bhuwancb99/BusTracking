namespace BusTracking.Common.Services;

public class ImageService : IImageService
{
    private readonly string _wwwRoot;

    public ImageService(IWebHostEnvironment env)
    {
        _wwwRoot = env.WebRootPath;
    }

    // ── Profile image (single per user — auto-replaces old on re-upload) ──
    public async Task<string> SaveProfileImageAsync(
        IFormFile file, int userId, string role, string? existingUrl)
    {
        Validate(file);

        var folder = Path.Combine(_wwwRoot, "images", role.ToLower());
        Directory.CreateDirectory(folder);

        // Delete ALL previous files for this user in this folder
        // regardless of extension (handles jpg→png swaps cleanly)
        DeleteAllUserFiles(folder, userId);

        var ext = GetSafeExtension(file.FileName);
        var fileName = $"u_{userId}{ext}";
        var fullPath = Path.Combine(folder, fileName);

        await SaveFileAsync(file, fullPath);

        return $"/images/{role.ToLower()}/{fileName}";
    }

    // ── Bus image (multiple allowed per bus) ──────────────────────────
    public async Task<string> SaveBusImageAsync(IFormFile file, int busId, int imageIndex)
    {
        Validate(file);

        var folder = Path.Combine(_wwwRoot, "images", "bus");
        Directory.CreateDirectory(folder);

        var ext = GetSafeExtension(file.FileName);
        var fileName = $"b_{busId}_{imageIndex}{ext}";
        var fullPath = Path.Combine(folder, fileName);

        await SaveFileAsync(file, fullPath);

        return $"/images/bus/{fileName}";
    }

    // ── Delete by public URL ──────────────────────────────────────────
    public void DeleteFile(string? publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl)) return;

        var relative = publicUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_wwwRoot, relative);

        try { if (File.Exists(fullPath)) File.Delete(fullPath); }
        catch { /* never crash on delete */ }
    }

    // ── Private helpers ───────────────────────────────────────────────

    /// <summary>
    /// Deletes all files matching u_{userId}.* in the folder.
    /// Handles the case where the user uploaded jpg before and now uploads png.
    /// </summary>
    private void DeleteAllUserFiles(string folder, int userId)
    {
        try
        {
            var pattern = $"u_{userId}.*";
            foreach (var file in Directory.GetFiles(folder, pattern))
            {
                try { File.Delete(file); }
                catch { /* skip locked files, never crash */ }
            }
        }
        catch { /* folder may not exist yet on very first upload */ }
    }

    private static async Task SaveFileAsync(IFormFile file, string fullPath)
    {
        using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await file.CopyToAsync(stream);
    }

    private static string GetSafeExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLower();
        return ext is ".jpg" or ".jpeg" or ".png" or ".webp" ? ext : ".jpg";
    }

    private static void Validate(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new InvalidOperationException("No file selected.");

        if (file.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("File must be under 5 MB.");

        var allowed = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType.ToLower()))
            throw new InvalidOperationException("Only JPG, PNG or WebP images are allowed.");
    }
}
