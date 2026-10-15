using DeliverySac.Api.Domain;
using DeliverySac.Api.Domain.Enums;
using DeliverySac.Api.Infrastructure;
using DeliverySac.Api.Infrastructure.Seed;
using DeliverySac.Api.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace DeliverySac.Tests.Integration;

public sealed class DeliverySacApiFactory : WebApplicationFactory<Program>
{
    public const string AdminUser = "admin.test";
    public const string AdminPassword = "Clave.123";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Key", "clave-de-pruebas-para-firmar-tokens-0123456789");
        builder.UseSetting("Jwt:Issuer", "DeliverySac");
        builder.UseSetting("Jwt:Audience", "DeliverySac");

        builder.ConfigureServices(services =>
        {
            RemoveAll(services, typeof(DbContextOptions<DeliverySacDbContext>));
            RemoveAll(services, typeof(DbContextOptions));

            services.AddDbContext<DeliverySacDbContext>(options =>
                options.UseSqlServer(ResolveConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DeliverySacDbContext>();

        await db.Database.MigrateAsync();

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        await DbSeeder.SeedAsync(db, hasher, AdminPassword, NullLogger.Instance);

        if (!await db.Unidades.AnyAsync())
        {
            db.Unidades.Add(new Unidad
            {
                Identificador = "MOTO-TEST",
                Tipo = TipoUnidad.MotoLineal,
                Activo = true,
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Usuarios.AnyAsync(u => u.Rol == RolUsuario.Repartidor))
        {
            db.Usuarios.Add(new Usuario
            {
                NombreUsuario = "repartidor.test",
                NombreCompleto = "Repartidor de Prueba",
                PasswordHash = hasher.Hash(AdminPassword),
                Rol = RolUsuario.Repartidor,
                Activo = true,
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Usuarios.AnyAsync(u => u.NombreUsuario == AdminUser))
        {
            db.Usuarios.Add(new Usuario
            {
                NombreUsuario = AdminUser,
                NombreCompleto = "Administrador de Prueba",
                PasswordHash = hasher.Hash(AdminPassword),
                Rol = RolUsuario.Administrador,
                Activo = true,
            });

            await db.SaveChangesAsync();
        }
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            nombreUsuario = AdminUser,
            password = AdminPassword,
        });

        loginResponse.EnsureSuccessStatusCode();

        var payload = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = payload.GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    private static string ResolveConnectionString()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("DELIVERYSAC_CONNECTION");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment;
        }

        return "Server=.\\MSSQLSERVER01;Database=DeliverySac_Test;Trusted_Connection=True;TrustServerCertificate=True;";
    }

    private static void RemoveAll(IServiceCollection services, Type serviceType)
    {
        var descriptors = services.Where(d => d.ServiceType == serviceType).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
