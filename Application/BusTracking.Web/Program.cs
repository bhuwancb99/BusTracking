var builder = WebApplication.CreateBuilder(args);


var apiMediaPath = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "..", "BusTracking.API", "media"));

builder.Configuration["MediaStorage:BasePath"] = apiMediaPath;

builder.Services.AddCommonServices(builder.Configuration);
builder.Services.AddControllersWithViews();

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
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Auth/AccessDenied");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles(); // wwwroot (css, js, bootstrap)

// ── Serve /media/* from BusTracking.API/media/ ────────────────────────
// Same physical folder as API → image uploaded via API shows on Web page
Directory.CreateDirectory(apiMediaPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(apiMediaPath),
    RequestPath = "/media"
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
