using DeliverySac.Api.Application;
using DeliverySac.Api.Domain;
using DeliverySac.Api.Domain.Enums;
using DeliverySac.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DeliverySac.Tests.Unit;

public class IdempotenciaTests
{
    private static DeliverySacDbContext NuevaDb()
    {
        var options = new DbContextOptionsBuilder<DeliverySacDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DeliverySacDbContext(options);
    }

    private static async Task SeedPedidoAsignadoAsync(DeliverySacDbContext db)
    {
        db.Unidades.Add(new Unidad { Id = 1, Identificador = "U-1", Tipo = TipoUnidad.MotoLineal, Activo = true });
        db.Usuarios.Add(new Usuario
        {
            Id = 1,
            NombreUsuario = "repartidor",
            NombreCompleto = "Repartidor",
            PasswordHash = "x",
            Rol = RolUsuario.Repartidor,
            Activo = true,
        });
        db.Clientes.Add(new Cliente { Id = 1, Nombre = "Cliente", Direccion = "Dir" });
        db.Pedidos.Add(new Pedido
        {
            Id = 1,
            ClienteId = 1,
            FechaReparto = new DateOnly(2026, 10, 15),
            Estado = EstadoPedido.Asignado,
            Total = 10m,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        db.Asignaciones.Add(new Asignacion
        {
            Id = 1,
            PedidoId = 1,
            RepartidorId = 1,
            UnidadId = 1,
            Fecha = new DateOnly(2026, 10, 15),
            Activa = true,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task OperacionRepetida_NoDuplicaTraza_YDevuelveOk()
    {
        await using var db = NuevaDb();
        await SeedPedidoAsignadoAsync(db);
        var service = new EstadoPedidoService(db);
        var operacionId = Guid.NewGuid();

        var primero = await service.CambiarEstadoAsync(
            1, EstadoPedido.EnRuta, null, 1, operacionId, CancellationToken.None);
        var repetido = await service.CambiarEstadoAsync(
            1, EstadoPedido.EnRuta, null, 1, operacionId, CancellationToken.None);

        Assert.Equal(ResultadoCambioEstado.Ok, primero);
        Assert.Equal(ResultadoCambioEstado.Ok, repetido);
        Assert.Equal(1, await db.CambiosEstado.CountAsync());
    }
}
