var builder = WebApplication.CreateBuilder(args);

// ── Inject API content root into config so ImageService can find it ───
// This means Web project also resolves to API's media folder correctly
// because the path is explicit, not derived from env.ContentRootPath
builder.Configuration["MediaStorage:BasePath"] =
    Path.Combine(builder.Environment.ContentRootPath, "media");

builder.Services.AddCommonServices(builder.Configuration);
builder.Services.AddControllers();

// Raise request size limit for image uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 26_214_400; // 5 × 5 MB
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

// ── Serve /media/* from BusTracking.API/media/ ────────────────────────
var mediaFolder = builder.Configuration["MediaStorage:BasePath"]!;
Directory.CreateDirectory(mediaFolder); // safe if exists
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaFolder),
    RequestPath = "/media"
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
