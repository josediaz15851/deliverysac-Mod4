using System.Diagnostics;

namespace DeliverySac.Tests.Integration;

public class PerformanceTests : IClassFixture<DeliverySacApiFactory>
{
    private static readonly TimeSpan Umbral = TimeSpan.FromSeconds(2);

    private readonly DeliverySacApiFactory _factory;

    public PerformanceTests(DeliverySacApiFactory factory)
    {
        _factory = factory;
        factory.InitializeAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task ListadoPaginado_RespondeDentroDelUmbral()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync();

        var cronometro = Stopwatch.StartNew();
        var response = await admin.GetAsync("/api/pedidos?page=1&size=10");
        cronometro.Stop();

        response.EnsureSuccessStatusCode();
        Assert.True(
            cronometro.Elapsed < Umbral,
            $"El listado tardó {cronometro.Elapsed.TotalSeconds:F2}s (umbral {Umbral.TotalSeconds:F0}s).");
    }

    [Fact]
    public async Task PanelDelDia_RespondeDentroDelUmbral()
    {
        var admin = await _factory.CreateAuthenticatedClientAsync();

        var cronometro = Stopwatch.StartNew();
        var response = await admin.GetAsync("/api/pedidos/dia?page=1&size=10");
        cronometro.Stop();

        response.EnsureSuccessStatusCode();
        Assert.True(
            cronometro.Elapsed < Umbral,
            $"El panel del día tardó {cronometro.Elapsed.TotalSeconds:F2}s (umbral {Umbral.TotalSeconds:F0}s).");
    }
}
