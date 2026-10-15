using System.Net;
using System.Net.Http.Json;

namespace DeliverySac.Tests.Integration;

public class ClientesEndpointsTests : IClassFixture<DeliverySacApiFactory>
{
    private readonly DeliverySacApiFactory _factory;

    public ClientesEndpointsTests(DeliverySacApiFactory factory)
    {
        _factory = factory;
        factory.InitializeAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task CrearCliente_ConDatosValidos_DevuelveClienteCreado()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/clientes", new
        {
            nombre = "Ana Torres",
            direccion = "Calle Los Pinos 123",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ClienteResponse>();
        Assert.NotNull(payload);
        Assert.True(payload!.Id > 0);
        Assert.Equal("Ana Torres", payload.Nombre);
    }

    [Fact]
    public async Task CrearCliente_SinNombre_Devuelve400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/clientes", new
        {
            nombre = string.Empty,
            direccion = "Calle Los Pinos 123",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CrearCliente_SinDireccion_Devuelve400()
    {
        var client = await _factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/clientes", new
        {
            nombre = "Cliente Válido",
            direccion = string.Empty,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record ClienteResponse(int Id, string Nombre, string Direccion);
}
