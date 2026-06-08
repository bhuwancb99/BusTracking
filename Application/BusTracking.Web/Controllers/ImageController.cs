//   - MAX_BUS_IMAGES = 5 enforced server-side
//   - UploadBusImages (plural) accepts List<IFormFile> for multi-upload
//   - Returns remaining slots and clear error when limit hit

namespace BusTracking.Web.Controllers;

[Authorize]
public class ImageController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IImageService _img;
    private const int MAX_BUS_IMAGES = 5;

    public ImageController(AppDbContext db, IImageService img)
    {
        _db = db;
        _img = img;
    }

    // ── Role → folder ─────────────────────────────────────────────────
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
    //  OWN PROFILE
    // =================================================================

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(5_242_880)]
    public async Task<IActionResult> UploadMyProfile(IFormFile file)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return Json(new { success = false, message = "User not found." });
        try
        {
            var url = await _img.SaveProfileImageAsync(file, CurrentUserId, RoleToFolder(CurrentUserRole), user.ProfileImageUrl);
            user.ProfileImageUrl = url; user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Json(new { success = true, imageUrl = url });
        }
        catch (InvalidOperationException ex) { return Json(new { success = false, message = ex.Message }); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMyProfile()
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return Json(new { success = false });
        _img.DeleteFile(user.ProfileImageUrl);
        user.ProfileImageUrl = null; user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Json(new { success = true });
    }

    // =================================================================
    //  ANY USER PROFILE  (admin action)
    // =================================================================

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,BusCoordinator"), RequestSizeLimit(5_242_880)]
    public async Task<IActionResult> UploadUserProfile(int userId, IFormFile file)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null) return Json(new { success = false, message = "User not found." });
        try
        {
            var url = await _img.SaveProfileImageAsync(file, userId, RoleToFolder(user.Role?.RoleName ?? "users"), user.ProfileImageUrl);
            user.ProfileImageUrl = url; user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Json(new { success = true, imageUrl = url });
        }
        catch (InvalidOperationException ex) { return Json(new { success = false, message = ex.Message }); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    public async Task<IActionResult> DeleteUserProfile(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return Json(new { success = false });
        _img.DeleteFile(user.ProfileImageUrl);
        user.ProfileImageUrl = null; user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Json(new { success = true });
    }

    // =================================================================
    //  BUS IMAGES — multi-upload with 5-image hard limit
    // =================================================================

    /// <summary>
    /// Accepts multiple files at once. Enforces MAX_BUS_IMAGES = 5 total per bus.
    /// Returns per-file results so the client knows exactly which succeeded/failed.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    [RequestSizeLimit(26_214_400)] // 5 files × 5 MB
    public async Task<IActionResult> UploadBusImages(int busId, List<IFormFile> files)
    {
        var bus = await _db.Buses
            .Include(b => b.Images)
            .FirstOrDefaultAsync(b => b.BusId == busId);

        if (bus is null)
            return Json(new { success = false, message = "Bus not found." });

        int existing = bus.Images.Count;
        int remaining = MAX_BUS_IMAGES - existing;

        // Already full
        if (remaining <= 0)
            return Json(new
            {
                success = false,
                limitHit = true,
                message = $"This bus already has {MAX_BUS_IMAGES} photos. Delete one to upload more."
            });

        // More files selected than slots available
        if (files.Count > remaining)
            return Json(new
            {
                success = false,
                limitHit = true,
                message = $"You can only upload {remaining} more photo{(remaining == 1 ? "" : "s")} " +
                            $"(bus limit is {MAX_BUS_IMAGES}). Please select fewer files."
            });

        var uploaded = new List<object>();
        var failed = new List<string>();
        int nextIndex = bus.Images.Any() ? bus.Images.Max(i => ExtractIndex(i.ImageUrl)) + 1 : 1;

        foreach (var file in files)
        {
            try
            {
                var url = await _img.SaveBusImageAsync(file, busId, nextIndex);

                bool isFirst = !bus.Images.Any() && uploaded.Count == 0;

                var busImage = new BusImage
                {
                    BusId = busId,
                    ImageUrl = url,
                    DisplayOrder = nextIndex,
                    IsPrimary = isFirst,
                    UploadedBy = CurrentUserId
                };

                _db.BusImages.Add(busImage);
                await _db.SaveChangesAsync();

                bus.Images.Add(busImage); // keep local list up to date
                nextIndex++;

                uploaded.Add(new
                {
                    busImageId = busImage.BusImageId,
                    imageUrl = url,
                    isPrimary = busImage.IsPrimary
                });
            }
            catch (InvalidOperationException ex)
            {
                failed.Add($"{file.FileName}: {ex.Message}");
            }
        }

        int newTotal = existing + uploaded.Count;

        return Json(new
        {
            success = uploaded.Count > 0,
            uploaded,
            failed,
            totalNow = newTotal,
            remaining = MAX_BUS_IMAGES - newTotal,
            message = uploaded.Count > 0
                ? $"{uploaded.Count} photo{(uploaded.Count == 1 ? "" : "s")} uploaded successfully."
                : "No photos were uploaded."
        });
    }

    // POST /Image/DeleteBusImage?busImageId=7
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    public async Task<IActionResult> DeleteBusImage(int busImageId)
    {
        var img = await _db.BusImages
            .Include(i => i.Bus).ThenInclude(b => b.Images)
            .FirstOrDefaultAsync(i => i.BusImageId == busImageId);

        if (img is null) return Json(new { success = false, message = "Image not found." });

        _img.DeleteFile(img.ImageUrl);
        _db.BusImages.Remove(img);
        await _db.SaveChangesAsync();

        var remaining = img.Bus.Images
            .Where(i => i.BusImageId != busImageId)
            .OrderBy(i => i.DisplayOrder).ToList();

        if (img.IsPrimary && remaining.Any())
        {
            remaining[0].IsPrimary = true;
            await _db.SaveChangesAsync();
        }

        int newTotal = remaining.Count;

        return Json(new
        {
            success = true,
            totalNow = newTotal,
            remaining = MAX_BUS_IMAGES - newTotal
        });
    }

    // POST /Image/SetPrimaryBusImage?busImageId=7
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,BusCoordinator")]
    public async Task<IActionResult> SetPrimaryBusImage(int busImageId)
    {
        var img = await _db.BusImages.FirstOrDefaultAsync(i => i.BusImageId == busImageId);
        if (img is null) return Json(new { success = false });

        var others = await _db.BusImages.Where(i => i.BusId == img.BusId && i.IsPrimary).ToListAsync();
        foreach (var o in others) o.IsPrimary = false;
        img.IsPrimary = true;
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // ── Helper ────────────────────────────────────────────────────────
    private static int ExtractIndex(string url)
    {
        try { var p = Path.GetFileNameWithoutExtension(url).Split('_'); return int.TryParse(p[^1], out var n) ? n : 0; }
        catch { return 0; }
    }
}
