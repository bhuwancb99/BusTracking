var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCommonServices(builder.Configuration);
builder.Services.AddControllers();

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 26_214_400; // 5 files × 5 MB
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
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(o => o.AddPolicy("AllowAll", p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

//builder.WebHost.UseUrls("https://0.0.0.0:7001", "http://0.0.0.0:5001");

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Serve BusTracking.API/media/ at /media/*
// Subfolders (superadmin, coordinator, driver, student, parent, bus)
// are created by ImageService constructor on first request.
var mediaFolder = Path.Combine(builder.Environment.ContentRootPath, "media");
Directory.CreateDirectory(mediaFolder);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaFolder),
    RequestPath = "/media"
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
