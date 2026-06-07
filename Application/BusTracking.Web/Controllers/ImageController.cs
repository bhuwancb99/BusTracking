// BusTracking.Web/Controllers/ImageController.cs
// Handles ALL image uploads / deletes from the web admin panel.
// No NuGet packages needed — ImageService uses only built-in .NET.

namespace BusTracking.Web.Controllers;

[Authorize]
public class ImageController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IImageService _img;

    public ImageController(AppDbContext db, IImageService img)
    {
        _db = db;
        _img = img;
    }

    // ── Helper: map role string → folder name ─────────────────────────
    private static string RoleToFolder(string role) => role.ToLower() switch
    {
        "superadmin" => "superadmin",
        "buscoordinator" => "coordinator",
        "driver" => "driver",
        "student" => "student",
        "parent" => "parent",
        _ => "users"
    };

    // =================================================================
    //  OWN PROFILE  (any logged-in user uploading their own photo)
    // =================================================================

    // POST /Image/UploadMyProfile
    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(5_242_880)]
    public async Task<IActionResult> UploadMyProfile(IFormFile file)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null)
            return Json(new { success = false, message = "User not found." });

        try
        {
            var folder = RoleToFolder(CurrentUserRole);
            var url = await _img.SaveProfileImageAsync(file, CurrentUserId, folder, user.ProfileImageUrl);

            user.ProfileImageUrl = url;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, imageUrl = url });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST /Image/DeleteMyProfile
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMyProfile()
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null)
            return Json(new { success = false });

        _img.DeleteFile(user.ProfileImageUrl);
        user.ProfileImageUrl = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // =================================================================
    //  ANY USER's PROFILE  (admin/coordinator managing another user)
    // =================================================================

    // POST /Image/UploadUserProfile?userId=42
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    [RequestSizeLimit(5_242_880)]
    public async Task<IActionResult> UploadUserProfile(int userId, IFormFile file)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null)
            return Json(new { success = false, message = "User not found." });

        try
        {
            var folder = RoleToFolder(user.Role?.RoleName ?? "users");
            var url = await _img.SaveProfileImageAsync(file, userId, folder, user.ProfileImageUrl);

            user.ProfileImageUrl = url;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, imageUrl = url });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST /Image/DeleteUserProfile?userId=42
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    public async Task<IActionResult> DeleteUserProfile(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return Json(new { success = false });

        _img.DeleteFile(user.ProfileImageUrl);
        user.ProfileImageUrl = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // =================================================================
    //  BUS IMAGES  (multiple images per bus)
    // =================================================================

    // POST /Image/UploadBusImage?busId=12
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    [RequestSizeLimit(5_242_880)]
    public async Task<IActionResult> UploadBusImage(int busId, IFormFile file, bool isPrimary = false)
    {
        var bus = await _db.Buses
            .Include(b => b.Images)
            .FirstOrDefaultAsync(b => b.BusId == busId);

        if (bus is null)
            return Json(new { success = false, message = "Bus not found." });

        try
        {
            var nextIndex = bus.Images.Any()
                ? bus.Images.Max(i => ExtractIndex(i.ImageUrl)) + 1
                : 1;

            var url = await _img.SaveBusImageAsync(file, busId, nextIndex);

            // First image is always primary automatically
            if (isPrimary || !bus.Images.Any())
            {
                foreach (var img in bus.Images.Where(i => i.IsPrimary))
                    img.IsPrimary = false;
                isPrimary = true;
            }

            var busImage = new BusImage
            {
                BusId = busId,
                ImageUrl = url,
                DisplayOrder = nextIndex,
                IsPrimary = isPrimary,
                UploadedBy = CurrentUserId
            };

            _db.BusImages.Add(busImage);
            await _db.SaveChangesAsync();

            return Json(new
            {
                success = true,
                busImageId = busImage.BusImageId,
                imageUrl = url,
                isPrimary = busImage.IsPrimary
            });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // POST /Image/DeleteBusImage?busImageId=7
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    public async Task<IActionResult> DeleteBusImage(int busImageId)
    {
        var img = await _db.BusImages
            .Include(i => i.Bus).ThenInclude(b => b.Images)
            .FirstOrDefaultAsync(i => i.BusImageId == busImageId);

        if (img is null)
            return Json(new { success = false, message = "Image not found." });

        _img.DeleteFile(img.ImageUrl);
        _db.BusImages.Remove(img);
        await _db.SaveChangesAsync();

        // Promote next image to primary if we deleted the primary
        var remaining = img.Bus.Images
            .Where(i => i.BusImageId != busImageId)
            .OrderBy(i => i.DisplayOrder)
            .ToList();

        if (img.IsPrimary && remaining.Any())
        {
            remaining[0].IsPrimary = true;
            await _db.SaveChangesAsync();
        }

        return Json(new { success = true });
    }

    // POST /Image/SetPrimaryBusImage?busImageId=7
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    public async Task<IActionResult> SetPrimaryBusImage(int busImageId)
    {
        var img = await _db.BusImages.FirstOrDefaultAsync(i => i.BusImageId == busImageId);
        if (img is null) return Json(new { success = false });

        var others = await _db.BusImages
            .Where(i => i.BusId == img.BusId && i.IsPrimary)
            .ToListAsync();

        foreach (var o in others) o.IsPrimary = false;
        img.IsPrimary = true;
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // ── Private helper ────────────────────────────────────────────────
    private static int ExtractIndex(string url)
    {
        try
        {
            var name = Path.GetFileNameWithoutExtension(url); // b_12_3
            var parts = name.Split('_');
            return int.TryParse(parts[^1], out var n) ? n : 0;
        }
        catch { return 0; }
    }
}
