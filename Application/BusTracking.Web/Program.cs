using BusTracking.Common;
using BusTracking.Common.Data;
using BusTracking.Common.Entities;
using BusTracking.Common.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Shared services from Common library ──────────────────────────────
builder.Services.AddCommonServices(builder.Configuration);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ── Cookie auth (Web portal) ─────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "BusTrack.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;   // works on both HTTP + HTTPS
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

//await SeedSuperAdminAsync(app);

// Never use exception handler or HTTPS redirect in development
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Auth/AccessDenied");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Area route MUST come before default
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();

// ── Seed SuperAdmin ───────────────────────────────────────────────────
static async Task SeedSuperAdminAsync(WebApplication app)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pwd = scope.ServiceProvider.GetRequiredService<IPasswordService>();

        if (await db.Users.AnyAsync()) return;

        var cfg = app.Configuration;
        var email = cfg["Seed:SuperAdminEmail"] ?? "admin@bustracking.com";
        var pass = cfg["Seed:SuperAdminPassword"] ?? "Admin@123";
        var name = cfg["Seed:SuperAdminName"] ?? "Super Admin";

        var role = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "SuperAdmin");
        if (role is null)
        {
            app.Logger.LogWarning("SuperAdmin role not found — run BusTrackingApp_Database.sql first.");
            return;
        }

        var (hash, salt) = pwd.HashPassword(pass);
        db.Users.Add(new User
        {
            RoleId = role.RoleId,
            FullName = name,
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            IsEmailVerified = true,
            IsActive = true
        });
        await db.SaveChangesAsync();
        app.Logger.LogInformation("SuperAdmin seeded → {Email} / {Pass}", email, pass);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Seed failed — check DB connection.");
    }
}