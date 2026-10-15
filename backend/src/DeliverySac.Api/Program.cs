using System.Text;
using DeliverySac.Api.Infrastructure;
using DeliverySac.Api.Infrastructure.Seed;
using DeliverySac.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

const string CorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.Development.local.json",
    optional: true,
    reloadOnChange: false);

var connectionString = Environment.GetEnvironmentVariable("DELIVERYSAC_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddProblemDetails();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

var jwtKey = Environment.GetEnvironmentVariable("DELIVERYSAC_JWT_KEY")
    ?? builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DeliverySac";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DeliverySac";

if (!string.IsNullOrWhiteSpace(jwtKey))
{
    builder.Services.Configure<JwtOptions>(options =>
    {
        options.Issuer = jwtIssuer;
        options.Audience = jwtAudience;
        options.Key = jwtKey;
        options.ExpiresMinutes = int.TryParse(builder.Configuration["Jwt:ExpiresMinutes"], out var minutes)
            ? minutes
            : 480;
    });

    builder.Services.AddSingleton<ITokenService, JwtTokenService>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            };
        });

    builder.Services.AddAuthorization();
}

if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<DeliverySacDbContext>(options =>
        options.UseSqlServer(connectionString));
}

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapControllers();

await SeedDatabaseAsync(app);

app.Run();

static async Task SeedDatabaseAsync(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        return;
    }

    using var scope = app.Services.CreateScope();
    var scopedDb = scope.ServiceProvider.GetService<DeliverySacDbContext>();
    if (scopedDb is null)
    {
        return;
    }

    await scopedDb.Database.MigrateAsync();

    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var adminPassword = Environment.GetEnvironmentVariable("DELIVERYSAC_ADMIN_PASSWORD");

    await DbSeeder.SeedAsync(scopedDb, hasher, adminPassword, logger);
}

public partial class Program { }
