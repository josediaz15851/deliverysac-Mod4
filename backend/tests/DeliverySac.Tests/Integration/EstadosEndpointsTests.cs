using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DeliverySac.Tests.Integration;

public class EstadosEndpointsTests : IClassFixture<DeliverySacApiFactory>
{
    private readonly DeliverySacApiFactory _factory;

    public EstadosEndpointsTests(DeliverySacApiFactory factory)
    {
        _factory = factory;
        factory.InitializeAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task FlujoCompleto_RegistraTrazabilidadConUsuarioYHora()
    {
        var (pedidoId, repartidor) = await PrepararPedidoAsignadoAsync();

        var enRuta = await repartidor.PostAsJsonAsync($"/api/pedidos/{pedidoId}/estados", new
        {
            estadoNuevo = "EnRuta",
        });
        Assert.Equal(HttpStatusCode.OK, enRuta.StatusCode);

        var entregado = await repartidor.PostAsJsonAsync($"/api/pedidos/{pedidoId}/estados", new
        {
            estadoNuevo = "Entregado",
        });
        Assert.Equal(HttpStatusCode.OK, entregado.StatusCode);

        var trazabilidad = await repartidor.GetFromJsonAsync<JsonElement>($"/api/pedidos/{pedidoId}/trazabilidad");
        var registros = trazabilidad.GetProperty("items").EnumerateArray().ToList();

        Assert.Contains(registros, r => r.GetProperty("estadoNuevo").GetString() == "EnRuta");
        Assert.Contains(registros, r => r.GetProperty("estadoNuevo").GetString() == "Entregado");
        var ultimo = registros.Last();
        Assert.False(string.IsNullOrWhiteSpace(ultimo.GetProperty("fechaHora").GetString()));
        Assert.Equal(DeliverySacApiFactory.RepartidorUser, ultimo.GetProperty("usuario").GetString());
    }

    [Fact]
    public async Task TransicionInvalida_Devuelve400()
    {
        var (pedidoId, repartidor) = await PrepararPedidoAsignadoAsync();

        var response = await repartidor.PostAsJsonAsync($"/api/pedidos/{pedidoId}/estados", new
        {
            estadoNuevo = "Entregado",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task NoEntregadoSinMotivo_Devuelve400()
    {
        var (pedidoId, repartidor) = await PrepararPedidoAsignadoAsync();

        await repartidor.PostAsJsonAsync($"/api/pedidos/{pedidoId}/estados", new { estadoNuevo = "EnRuta" });

        var response = await repartidor.PostAsJsonAsync($"/api/pedidos/{pedidoId}/estados", new
        {
            estadoNuevo = "NoEntregado",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task NoEntregadoConMotivo_Devuelve200()
    {
        var (pedidoId, repartidor) = await PrepararPedidoAsignadoAsync();

        await repartidor.PostAsJsonAsync($"/api/pedidos/{pedidoId}/estados", new { estadoNuevo = "EnRuta" });

        var response = await repartidor.PostAsJsonAsync($"/api/pedidos/{pedidoId}/estados", new
        {
            estadoNuevo = "NoEntregado",
            motivo = "El cliente no se encontraba en el domicilio",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RepartidorAjeno_Devuelve403()
    {
        var (pedidoId, _) = await PrepararPedidoAsignadoAsync();
        var otroRepartidor = await _factory.CreateAuthenticatedClientAsync(
            DeliverySacApiFactory.OtroRepartidorUser,
            DeliverySacApiFactory.AdminPassword);

        var response = await otroRepartidor.PostAsJsonAsync($"/api/pedidos/{pedidoId}/estados", new
        {
            estadoNuevo = "EnRuta",
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RutaDelRepartidor_SoloMuestraPedidosPropiosDelDia()
    {
        var (pedidoId, repartidor) = await PrepararPedidoAsignadoAsync();

        var ruta = await repartidor.GetFromJsonAsync<JsonElement>("/api/repartidor/pedidos");
        var items = ruta.EnumerateArray().ToList();

        Assert.Contains(items, item => item.GetProperty("id").GetInt32() == pedidoId);
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

        var repartidorId = await ObtenerRepartidorIdAsync(admin, DeliverySacApiFactory.RepartidorUser);
        var unidadId = await ObtenerPrimerIdAsync(admin, "/api/unidades");

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

    private static async Task<int> ObtenerRepartidorIdAsync(HttpClient client, string nombreUsuario)
    {
        var repartidores = await client.GetFromJsonAsync<JsonElement>("/api/repartidores");
        return repartidores.EnumerateArray()
            .First(r => r.GetProperty("nombreUsuario").GetString() == nombreUsuario)
            .GetProperty("id").GetInt32();
    }

    private static async Task<int> ObtenerPrimerIdAsync(HttpClient client, string url)
    {
        var items = await client.GetFromJsonAsync<JsonElement>(url);
        return items.EnumerateArray().First().GetProperty("id").GetInt32();
    }
}
