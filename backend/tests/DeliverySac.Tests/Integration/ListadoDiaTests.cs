using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DeliverySac.Tests.Integration;

public class ListadoDiaTests : IClassFixture<DeliverySacApiFactory>
{
    private readonly DeliverySacApiFactory _factory;

    public ListadoDiaTests(DeliverySacApiFactory factory)
    {
        _factory = factory;
        factory.InitializeAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Dia_IncluyePedidosDeHoyYReprogramadosDeDiasPrevios()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync();
        var hoyId = await CrearPedidoAsync(admin, DateOnly.FromDateTime(DateTime.UtcNow));
        var ayerId = await CrearPedidoReprogramadoAsync(admin, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));

        var dia = await admin.GetFromJsonAsync<JsonElement>("/api/pedidos/dia?page=1&size=50");
        var items = dia.GetProperty("items").EnumerateArray().ToList();

        Assert.Contains(items, item => item.GetProperty("id").GetInt32() == hoyId);

        var reprogramado = items.FirstOrDefault(item => item.GetProperty("id").GetInt32() == ayerId);
        Assert.NotEqual(default, reprogramado);
        Assert.Equal("Reprogramado", reprogramado.GetProperty("estado").GetString());
        Assert.False(string.IsNullOrWhiteSpace(reprogramado.GetProperty("motivo").GetString()));
    }

    [Fact]
    public async Task Repartidor_NoAccedeAlPanel_Devuelve403()
    {
        var repartidor = await _factory.CreateAuthenticatedClientAsync(
            DeliverySacApiFactory.RepartidorUser,
            DeliverySacApiFactory.AdminPassword);

        var response = await repartidor.GetAsync("/api/pedidos/dia");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<int> CrearPedidoAsync(HttpClient admin, DateOnly fecha)
    {
        var clienteId = await CrearClienteAsync(admin);
        var response = await admin.PostAsJsonAsync("/api/pedidos", new
        {
            clienteId,
            fechaReparto = fecha.ToString("yyyy-MM-dd"),
            lineas = new object[] { new { producto = "A", cantidad = 1, precioUnitario = 5.00m, descripcion = "uno A" } },
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }

    private async Task<int> CrearPedidoReprogramadoAsync(HttpClient admin, DateOnly fecha)
    {
        var pedidoId = await CrearPedidoAsync(admin, fecha);

        var repartidores = await admin.GetFromJsonAsync<JsonElement>("/api/repartidores");
        var repartidorId = repartidores.EnumerateArray()
            .First(r => r.GetProperty("nombreUsuario").GetString() == DeliverySacApiFactory.RepartidorUser)
            .GetProperty("id").GetInt32();

        var unidades = await admin.GetFromJsonAsync<JsonElement>("/api/unidades");
        var unidadId = unidades.EnumerateArray().First().GetProperty("id").GetInt32();

        var asignacion = await admin.PostAsJsonAsync($"/api/pedidos/{pedidoId}/asignaciones", new { repartidorId, unidadId });
        asignacion.EnsureSuccessStatusCode();

        var repartidor = await _factory.CreateAuthenticatedClientAsync(
            DeliverySacApiFactory.RepartidorUser,
            DeliverySacApiFactory.AdminPassword);

        await repartidor.PostAsJsonAsync($"/api/pedidos/{pedidoId}/estados", new { estadoNuevo = "EnRuta" });
        var reprogramado = await repartidor.PostAsJsonAsync($"/api/pedidos/{pedidoId}/estados", new
        {
            estadoNuevo = "Reprogramado",
            motivo = "El cliente pidió otro día",
        });
        reprogramado.EnsureSuccessStatusCode();

        return pedidoId;
    }

    private static async Task<int> CrearClienteAsync(HttpClient admin)
    {
        var response = await admin.PostAsJsonAsync("/api/clientes", new
        {
            nombre = $"Cliente {Guid.NewGuid():N}",
            direccion = "Dirección de prueba",
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }
}
