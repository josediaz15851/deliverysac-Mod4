using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DeliverySac.Tests.Integration;

public class OfflineSyncTests : IClassFixture<DeliverySacApiFactory>
{
    private readonly DeliverySacApiFactory _factory;

    public OfflineSyncTests(DeliverySacApiFactory factory)
    {
        _factory = factory;
        factory.InitializeAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task CambioEncoladoSeEnviaAlReconectar_YNoDuplicaLaTraza()
    {
        var (pedidoId, repartidor) = await PrepararPedidoAsignadoAsync();
        var operacionId = Guid.NewGuid();

        var primerEnvio = await repartidor.PostAsJsonAsync($"/api/pedidos/{pedidoId}/estados", new
        {
            estadoNuevo = "EnRuta",
            operacionId,
        });

        var reenvio = await repartidor.PostAsJsonAsync($"/api/pedidos/{pedidoId}/estados", new
        {
            estadoNuevo = "EnRuta",
            operacionId,
        });

        Assert.Equal(HttpStatusCode.OK, primerEnvio.StatusCode);
        Assert.Equal(HttpStatusCode.OK, reenvio.StatusCode);

        var trazabilidad = await repartidor.GetFromJsonAsync<JsonElement>($"/api/pedidos/{pedidoId}/trazabilidad");
        var items = trazabilidad.GetProperty("items").EnumerateArray().ToList();

        Assert.Single(items, item => item.GetProperty("estadoNuevo").GetString() == "EnRuta");
    }

    private async Task<(int PedidoId, HttpClient Repartidor)> PrepararPedidoAsignadoAsync()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync();

        var clienteResponse = await admin.PostAsJsonAsync("/api/clientes", new
        {
            nombre = $"Cliente {Guid.NewGuid():N}",
            direccion = "Dirección de prueba",
        });
        clienteResponse.EnsureSuccessStatusCode();
        var clienteId = (await clienteResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
        var pedidoResponse = await admin.PostAsJsonAsync("/api/pedidos", new
        {
            clienteId,
            fechaReparto = hoy,
            lineas = new object[]
            {
                new { producto = "A", cantidad = 1, precioUnitario = 5.00m, descripcion = "uno A" },
            },
        });
        pedidoResponse.EnsureSuccessStatusCode();
        var pedidoId = (await pedidoResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var repartidores = await admin.GetFromJsonAsync<JsonElement>("/api/repartidores");
        var repartidorId = repartidores.EnumerateArray()
            .First(r => r.GetProperty("nombreUsuario").GetString() == DeliverySacApiFactory.RepartidorUser)
            .GetProperty("id").GetInt32();

        var unidades = await admin.GetFromJsonAsync<JsonElement>("/api/unidades");
        var unidadId = unidades.EnumerateArray().First().GetProperty("id").GetInt32();

        var asignacion = await admin.PostAsJsonAsync($"/api/pedidos/{pedidoId}/asignaciones", new
        {
            repartidorId,
            unidadId,
        });
        asignacion.EnsureSuccessStatusCode();

        var repartidor = await _factory.CreateAuthenticatedClientAsync(
            DeliverySacApiFactory.RepartidorUser,
            DeliverySacApiFactory.AdminPassword);

        return (pedidoId, repartidor);
    }
}
