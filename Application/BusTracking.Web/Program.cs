var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCommonServices(builder.Configuration);

// Initialize FirebaseAdmin App for push notifications if service account key exists
var firebaseKeyPath = Path.Combine(builder.Environment.ContentRootPath, "firebase-service-account.json");
if (File.Exists(firebaseKeyPath) && FirebaseAdmin.FirebaseApp.DefaultInstance == null)
{
    try
    {
        FirebaseAdmin.FirebaseApp.Create(new FirebaseAdmin.AppOptions
        {
#pragma warning disable CS0618
            Credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromFile(firebaseKeyPath)
#pragma warning restore CS0618
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[FirebaseAdmin] Init Exception: {ex.Message}");
    }
}
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<BusTracking.Web.Filters.TenantActiveValidationFilter>();
});

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
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/SystemAdmin"))
                {
                    context.RedirectUri = "/SystemAdmin/Auth/Login";
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<BusTracking.Web.Middleware.WebExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Auth/AccessDenied");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles(); // wwwroot

// Serve BusTracking.Web/media/ at /media/*
var mediaFolder = Path.Combine(builder.Environment.ContentRootPath, "media");
Directory.CreateDirectory(mediaFolder);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaFolder),
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
