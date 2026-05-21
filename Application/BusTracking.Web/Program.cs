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

// Never use exception handler or HTTPS redirect in development
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Auth/AccessDenied");
    app.UseHsts();
    app.UseHttpsRedirection();
}

//var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images");
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(imagePath),
//    RequestPath = "/Images"
//});
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