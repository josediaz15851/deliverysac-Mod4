using DeliverySac.Api.Application;
using DeliverySac.Api.Domain.Enums;

namespace DeliverySac.Tests.Unit;

public class EstadoPedidoTests
{
    [Theory]
    [InlineData(EstadoPedido.Pendiente, EstadoPedido.Asignado)]
    [InlineData(EstadoPedido.Asignado, EstadoPedido.EnRuta)]
    [InlineData(EstadoPedido.EnRuta, EstadoPedido.Entregado)]
    [InlineData(EstadoPedido.EnRuta, EstadoPedido.NoEntregado)]
    [InlineData(EstadoPedido.EnRuta, EstadoPedido.Reprogramado)]
    [InlineData(EstadoPedido.Reprogramado, EstadoPedido.Asignado)]
    public void PuedeTransicionar_PermiteTransicionesValidas(EstadoPedido desde, EstadoPedido hacia)
    {
        Assert.True(EstadoPedidoService.PuedeTransicionar(desde, hacia));
    }

    [Theory]
    [InlineData(EstadoPedido.Pendiente, EstadoPedido.Entregado)]
    [InlineData(EstadoPedido.Pendiente, EstadoPedido.EnRuta)]
    [InlineData(EstadoPedido.Asignado, EstadoPedido.Entregado)]
    [InlineData(EstadoPedido.Entregado, EstadoPedido.Asignado)]
    [InlineData(EstadoPedido.Entregado, EstadoPedido.EnRuta)]
    [InlineData(EstadoPedido.NoEntregado, EstadoPedido.EnRuta)]
    public void PuedeTransicionar_RechazaTransicionesInvalidas(EstadoPedido desde, EstadoPedido hacia)
    {
        Assert.False(EstadoPedidoService.PuedeTransicionar(desde, hacia));
    }

    [Theory]
    [InlineData(EstadoPedido.Asignado)]
    [InlineData(EstadoPedido.EnRuta)]
    [InlineData(EstadoPedido.Entregado)]
    [InlineData(EstadoPedido.NoEntregado)]
    [InlineData(EstadoPedido.Reprogramado)]
    public void EntregadoEsTerminal(EstadoPedido hacia)
    {
        Assert.False(EstadoPedidoService.PuedeTransicionar(EstadoPedido.Entregado, hacia));
    }

    [Theory]
    [InlineData(EstadoPedido.NoEntregado, true)]
    [InlineData(EstadoPedido.Reprogramado, true)]
    [InlineData(EstadoPedido.Asignado, false)]
    [InlineData(EstadoPedido.EnRuta, false)]
    [InlineData(EstadoPedido.Entregado, false)]
    public void RequiereMotivo_SoloParaEstadosConMotivo(EstadoPedido estado, bool esperado)
    {
        Assert.Equal(esperado, EstadoPedidoService.RequiereMotivo(estado));
    }
}
