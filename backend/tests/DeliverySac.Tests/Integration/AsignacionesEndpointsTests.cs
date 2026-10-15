using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DeliverySac.Tests.Integration;

public class AsignacionesEndpointsTests : IClassFixture<DeliverySacApiFactory>
{
    private readonly DeliverySacApiFactory _factory;

    public AsignacionesEndpointsTests(DeliverySacApiFactory factory)
    {
        _factory = factory;
        factory.InitializeAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task AsignarPedidoPendiente_CreaAsignacionYCambiaEstado()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var pedidoId = await CrearPedidoAsync(client);
        var repartidorId = await ObtenerPrimerIdAsync(client, "/api/repartidores");
        var unidadId = await ObtenerPrimerIdAsync(client, "/api/unidades");

        var response = await client.PostAsJsonAsync($"/api/pedidos/{pedidoId}/asignaciones", new
        {
            repartidorId,
            unidadId,
        });

        Assert.True(
            response.StatusCode is HttpStatusCode.OK or HttpStatusCode.Created,
            $"Se esperaba éxito pero se obtuvo {(int)response.StatusCode}.");

        var detalle = await client.GetFromJsonAsync<JsonElement>($"/api/pedidos/{pedidoId}");
        Assert.Equal("Asignado", detalle.GetProperty("estado").GetString());
    }

    [Fact]
    public async Task AsignarPedidoYaAsignado_Devuelve400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var pedidoId = await CrearPedidoAsync(client);
        var repartidorId = await ObtenerPrimerIdAsync(client, "/api/repartidores");
        var unidadId = await ObtenerPrimerIdAsync(client, "/api/unidades");

        await client.PostAsJsonAsync($"/api/pedidos/{pedidoId}/asignaciones", new { repartidorId, unidadId });

        var segunda = await client.PostAsJsonAsync($"/api/pedidos/{pedidoId}/asignaciones", new
        {
            repartidorId,
            unidadId,
        });

        Assert.Equal(HttpStatusCode.BadRequest, segunda.StatusCode);
    }

    [Fact]
    public async Task Candidatos_IncluyePedidoPendiente()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var pedidoId = await CrearPedidoAsync(client);

        var candidatos = await client.GetFromJsonAsync<JsonElement>("/api/pedidos/candidatos?page=1&size=50");
        var items = candidatos.GetProperty("items").EnumerateArray().ToList();

        Assert.Contains(items, item => item.GetProperty("id").GetInt32() == pedidoId);
    }

    private static async Task<int> CrearPedidoAsync(HttpClient client)
    {
        var clienteResponse = await client.PostAsJsonAsync("/api/clientes", new
        {
            nombre = $"Cliente {Guid.NewGuid():N}",
            direccion = "Dirección de prueba",
        });
        clienteResponse.EnsureSuccessStatusCode();
        var clienteId = (await clienteResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var pedidoResponse = await client.PostAsJsonAsync("/api/pedidos", new
        {
            clienteId,
            fechaReparto = "2026-10-15",
            lineas = new object[]
            {
                new { producto = "A", cantidad = 1, precioUnitario = 5.00m, descripcion = "uno A" },
            },
        });
        pedidoResponse.EnsureSuccessStatusCode();

        return (await pedidoResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }

    private static async Task<int> ObtenerPrimerIdAsync(HttpClient client, string url)
    {
        var items = await client.GetFromJsonAsync<JsonElement>(url);
        return items.EnumerateArray().First().GetProperty("id").GetInt32();
    }
}
