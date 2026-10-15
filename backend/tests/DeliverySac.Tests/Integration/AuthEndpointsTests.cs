using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DeliverySac.Tests.Integration;

public class AuthEndpointsTests : IClassFixture<DeliverySacApiFactory>
{
    private readonly DeliverySacApiFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests(DeliverySacApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        factory.InitializeAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveTokenConRol()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            nombreUsuario = DeliverySacApiFactory.AdminUser,
            password = DeliverySacApiFactory.AdminPassword,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = payload.GetProperty("token").GetString();
        var rol = payload.GetProperty("usuario").GetProperty("rol").GetString();

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal("Administrador", rol);
    }

    [Fact]
    public async Task Login_ConCredencialesInvalidas_Devuelve401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            nombreUsuario = "usuario.inexistente",
            password = "clave-mala",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
