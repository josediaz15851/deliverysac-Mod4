using DeliverySac.Api.Application;
using DeliverySac.Api.Domain;

namespace DeliverySac.Tests.Unit;

public class PedidoTests
{
    [Fact]
    public void CalcularTotal_SumaCantidadPorPrecio()
    {
        var lineas = new[]
        {
            new PedidoLinea { Producto = "A", Cantidad = 2, PrecioUnitario = 2.50m, Descripcion = "dos A" },
            new PedidoLinea { Producto = "B", Cantidad = 1, PrecioUnitario = 7.00m, Descripcion = "uno B" },
        };

        var total = PedidoCalculator.CalcularTotal(lineas);

        Assert.Equal(12.00m, total);
    }

    [Fact]
    public void EsLineaValida_RechazaCantidadNoPositiva()
    {
        var linea = new PedidoLinea { Producto = "A", Cantidad = 0, PrecioUnitario = 1m, Descripcion = "x" };

        Assert.False(PedidoCalculator.EsLineaValida(linea));
    }

    [Fact]
    public void EsLineaValida_RechazaPrecioNegativo()
    {
        var linea = new PedidoLinea { Producto = "A", Cantidad = 1, PrecioUnitario = -0.01m, Descripcion = "x" };

        Assert.False(PedidoCalculator.EsLineaValida(linea));
    }

    [Fact]
    public void EsLineaValida_RechazaDescripcionVacia()
    {
        var linea = new PedidoLinea { Producto = "A", Cantidad = 1, PrecioUnitario = 1m, Descripcion = "" };

        Assert.False(PedidoCalculator.EsLineaValida(linea));
    }

    [Fact]
    public void SonLineasValidas_RechazaPedidoSinLineas()
    {
        Assert.False(PedidoCalculator.SonLineasValidas(Array.Empty<PedidoLinea>()));
    }
}
