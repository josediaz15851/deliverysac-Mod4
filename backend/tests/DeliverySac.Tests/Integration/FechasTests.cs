using System.Net.Http.Json;
using System.Text.Json;

namespace DeliverySac.Tests.Integration;

public class FechasTests : IClassFixture<DeliverySacApiFactory>
{
    private readonly DeliverySacApiFactory _factory;

    public FechasTests(DeliverySacApiFactory factory)
    {
        _factory = factory;
        factory.InitializeAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task FechaReparto_NoSufreCorrimientoUTC_YApareceEnElDia()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync();
        var clienteId = await CrearClienteAsync(admin);

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var fecha = hoy.ToString("yyyy-MM-dd");

        var creacion = await admin.PostAsJsonAsync("/api/pedidos", new
        {
            clienteId,
            fechaReparto = fecha,
            lineas = new object[] { new { producto = "A", cantidad = 1, precioUnitario = 5.00m, descripcion = "uno A" } },
        });
        creacion.EnsureSuccessStatusCode();
        var pedidoId = (await creacion.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();

        var detalle = await admin.GetFromJsonAsync<JsonElement>($"/api/pedidos/{pedidoId}");
        Assert.Equal(fecha, detalle.GetProperty("fechaReparto").GetString());

        var dia = await admin.GetFromJsonAsync<JsonElement>("/api/pedidos/dia?page=1&size=100");
        var items = dia.GetProperty("items").EnumerateArray().ToList();
        Assert.Contains(items, item => item.GetProperty("id").GetInt32() == pedidoId);
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
