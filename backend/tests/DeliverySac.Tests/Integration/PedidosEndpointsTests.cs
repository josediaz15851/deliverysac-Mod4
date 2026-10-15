using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DeliverySac.Tests.Integration;

public class PedidosEndpointsTests : IClassFixture<DeliverySacApiFactory>
{
    private readonly DeliverySacApiFactory _factory;

    public PedidosEndpointsTests(DeliverySacApiFactory factory)
    {
        _factory = factory;
        factory.InitializeAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task CrearPedido_CalculaTotalExactoYEstadoPendiente()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var clienteId = await CrearClienteAsync(client);

        var response = await client.PostAsJsonAsync("/api/pedidos", new
        {
            clienteId,
            fechaReparto = "2026-10-15",
            comentario = "Entrega al mediodía",
            lineas = new object[]
            {
                new { producto = "A", cantidad = 2, precioUnitario = 2.50m, descripcion = "dos A" },
                new { producto = "B", cantidad = 1, precioUnitario = 7.00m, descripcion = "uno B" },
            },
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var detalle = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(12.00m, detalle.GetProperty("total").GetDecimal());
        Assert.Equal("Pendiente", detalle.GetProperty("estado").GetString());
    }

    [Fact]
    public async Task CrearPedido_SinLineas_Devuelve400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var clienteId = await CrearClienteAsync(client);

        var response = await client.PostAsJsonAsync("/api/pedidos", new
        {
            clienteId,
            fechaReparto = "2026-10-15",
            lineas = Array.Empty<object>(),
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CrearPedido_ConLineaInvalida_Devuelve400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var clienteId = await CrearClienteAsync(client);

        var response = await client.PostAsJsonAsync("/api/pedidos", new
        {
            clienteId,
            fechaReparto = "2026-10-15",
            lineas = new object[]
            {
                new { producto = "A", cantidad = 0, precioUnitario = 2.50m, descripcion = "cantidad inválida" },
            },
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListarPedidos_IncluyePedidoPendienteSinAsignacion()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();
        var clienteId = await CrearClienteAsync(client);

        var creado = await client.PostAsJsonAsync("/api/pedidos", new
        {
            clienteId,
            fechaReparto = "2026-10-16",
            lineas = new object[]
            {
                new { producto = "A", cantidad = 1, precioUnitario = 5.00m, descripcion = "uno A" },
            },
        });
        var pedidoId = (await creado.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var listado = await client.GetFromJsonAsync<JsonElement>("/api/pedidos?page=1&size=50");
        var items = listado.GetProperty("items").EnumerateArray().ToList();

        Assert.Contains(items, item => item.GetProperty("id").GetInt32() == pedidoId
            && item.GetProperty("estado").GetString() == "Pendiente");
    }

    private static async Task<int> CrearClienteAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/clientes", new
        {
            nombre = $"Cliente {Guid.NewGuid():N}",
            direccion = "Dirección de prueba",
        });

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        return payload.GetProperty("id").GetInt32();
    }
}
