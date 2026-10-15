using DeliverySac.Api.Domain;
using DeliverySac.Api.Domain.Enums;
using DeliverySac.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace DeliverySac.Api.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(
        DeliverySacDbContext db,
        IPasswordHasher passwordHasher,
        string? adminPassword,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await SeedUnidadesAsync(db, logger, cancellationToken);
        await SeedClienteAsync(db, logger, cancellationToken);
        await SeedUsuariosAsync(db, passwordHasher, adminPassword, logger, cancellationToken);
    }

    private static async Task SeedUnidadesAsync(
        DeliverySacDbContext db,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await db.Unidades.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Unidades.AddRange(
            new Unidad { Identificador = "MOTO-01", Tipo = TipoUnidad.MotoLineal, Activo = true },
            new Unidad { Identificador = "MOTO-02", Tipo = TipoUnidad.MotoLineal, Activo = true },
            new Unidad { Identificador = "MOTO-03", Tipo = TipoUnidad.MotoLineal, Activo = true },
            new Unidad { Identificador = "FURGON-01", Tipo = TipoUnidad.Furgoneta, Activo = true },
            new Unidad { Identificador = "FURGON-02", Tipo = TipoUnidad.Furgoneta, Activo = true });

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Datos base: unidades sembradas (3 motos lineales, 2 furgonetas).");
    }

    private static async Task SeedClienteAsync(
        DeliverySacDbContext db,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await db.Clientes.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Clientes.Add(new Cliente
        {
            Nombre = "Cliente Demo",
            Direccion = "Av. Siempre Viva 742, Lima",
        });

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Datos base: cliente de demostración sembrado.");
    }

    private static async Task SeedUsuariosAsync(
        DeliverySacDbContext db,
        IPasswordHasher passwordHasher,
        string? adminPassword,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await db.Usuarios.AnyAsync(cancellationToken))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "Datos base: no se sembraron usuarios porque la variable DELIVERYSAC_ADMIN_PASSWORD no está definida.");
            return;
        }

        var hash = passwordHasher.Hash(adminPassword);

        db.Usuarios.AddRange(
            new Usuario
            {
                NombreUsuario = "admin",
                NombreCompleto = "Administrador del Sistema",
                PasswordHash = hash,
                Rol = RolUsuario.Administrador,
                Activo = true,
            },
            new Usuario
            {
                NombreUsuario = "repartidor",
                NombreCompleto = "Repartidor Demo",
                PasswordHash = hash,
                Rol = RolUsuario.Repartidor,
                Activo = true,
            },
            new Usuario
            {
                NombreUsuario = "supervisor",
                NombreCompleto = "Supervisor Demo",
                PasswordHash = hash,
                Rol = RolUsuario.Supervisor,
                Activo = true,
            });

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Datos base: usuarios por rol sembrados (admin, repartidor, supervisor).");
    }
}
