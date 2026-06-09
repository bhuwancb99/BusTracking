namespace BusTracking.Common.Services;

public class ImageService : IImageService
{
    private readonly string _mediaPath;
    private readonly IHttpContextAccessor _httpContext;

    public ImageService(IWebHostEnvironment env, IHttpContextAccessor httpContext)
    {
        _mediaPath = Path.Combine(env.ContentRootPath, "media", "images");
        _httpContext = httpContext;

        // Create all role subfolders on startup — silent if already exist
        foreach (var role in new[] { "superadmin", "coordinator", "driver", "student", "parent", "bus" })
            Directory.CreateDirectory(Path.Combine(_mediaPath, role));
    }

    private string GetBaseUrl()
    {
        var req = _httpContext.HttpContext?.Request;
        if (req is null)
            throw new InvalidOperationException("ImageService requires an active HTTP request context.");

        return $"{req.Scheme}://{req.Host}";
    }

    // ── Profile image (single — auto-replaces on re-upload) ──────────
    public async Task<string> SaveProfileImageAsync(
        IFormFile file, int userId, string role, string? existingUrl)
    {
        Validate(file);

        var folder = Path.Combine(_mediaPath, role.ToLower());
        Directory.CreateDirectory(folder);

        // Delete all existing files for this user (handles .jpg → .png swaps)
        DeleteAllUserFiles(folder, userId);

        var ext = GetSafeExtension(file.FileName);
        var fileName = $"u_{userId}{ext}";
        await SaveFileAsync(file, Path.Combine(folder, fileName));

        // Full URL auto-built from live request — no config needed
        return $"{GetBaseUrl()}/media/images/{role.ToLower()}/{fileName}";
    }

    // ── Bus image (multiple per bus) ──────────────────────────────────
    public async Task<string> SaveBusImageAsync(IFormFile file, int busId, int imageIndex)
    {
        Validate(file);

        var folder = Path.Combine(_mediaPath, "bus");
        Directory.CreateDirectory(folder);

        var ext = GetSafeExtension(file.FileName);
        var fileName = $"b_{busId}_{imageIndex}{ext}";
        await SaveFileAsync(file, Path.Combine(folder, fileName));

        return $"{GetBaseUrl()}/media/images/bus/{fileName}";
    }

    // ── Delete by full URL ────────────────────────────────────────────
    public void DeleteFile(string? publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl)) return;
        try
        {
            var uri = new Uri(publicUrl);
            var path = uri.AbsolutePath.TrimStart('/');
            if (path.StartsWith("media/images/", StringComparison.OrdinalIgnoreCase))
                path = path["media/images/".Length..];

            var fullPath = Path.Combine(_mediaPath, path.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
        catch { }
    }

    // ── Helpers ───────────────────────────────────────────────────────
    private void DeleteAllUserFiles(string folder, int userId)
    {
        try
        {
            foreach (var f in Directory.GetFiles(folder, $"u_{userId}.*"))
                try { File.Delete(f); } catch { }
        }
        catch { }
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
