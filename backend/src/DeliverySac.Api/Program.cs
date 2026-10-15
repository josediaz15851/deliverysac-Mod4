using DeliverySac.Api.Infrastructure;
using DeliverySac.Api.Infrastructure.Seed;
using DeliverySac.Api.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.Development.local.json",
    optional: true,
    reloadOnChange: false);

var connectionString = Environment.GetEnvironmentVariable("DELIVERYSAC_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<DeliverySacDbContext>(options =>
        options.UseSqlServer(connectionString));
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

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
