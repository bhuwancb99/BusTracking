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
builder.Services.AddControllers();

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 26_214_400;
});
builder.WebHost.ConfigureKestrel(k =>
{
    k.Limits.MaxRequestBodySize = 26_214_400;
});

var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // ── SignalR: accept JWT from query string (?access_token=...)
        // WebSocket connections cannot set HTTP headers, so the token
        // must travel in the URL. This is the standard ASP.NET Core pattern.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(token) &&
                    path.StartsWithSegments("/hubs"))
                {
                    ctx.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── SignalR ──────────────────────────────────────────────────────────────────
builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = builder.Environment.IsDevelopment();
    o.KeepAliveInterval = TimeSpan.FromSeconds(15);
    o.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// ── CORS — must use SetIsOriginAllowed + AllowCredentials for SignalR ────────
builder.Services.AddCors(o => o.AddPolicy("AllowAll", p =>
    p.SetIsOriginAllowed(_ => true)
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()));

builder.WebHost.UseUrls("https://0.0.0.0:7001", "http://0.0.0.0:5001");

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Serve BusTracking.API/media/ at /media/*
var mediaFolder = Path.Combine(builder.Environment.ContentRootPath, "media");
Directory.CreateDirectory(mediaFolder);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(mediaFolder),
    RequestPath = "/media"
});

app.UseExceptionMiddleware();
app.UseAuthentication();
app.UseTenantActiveValidation();
app.UseAuthorization();
app.MapControllers();

// ── SignalR hub endpoint ─────────────────────────────────────────────────────
app.MapHub<BusTracking.API.Hubs.TripTrackingHub>("/hubs/tracking");

app.Run();
