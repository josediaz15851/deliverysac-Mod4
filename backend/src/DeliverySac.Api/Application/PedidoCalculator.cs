using DeliverySac.Api.Domain;

namespace DeliverySac.Api.Application;

public static class PedidoCalculator
{
    public static decimal CalcularTotal(IEnumerable<PedidoLinea> lineas)
    {
        return lineas.Sum(linea => linea.Cantidad * linea.PrecioUnitario);
    }

    public static bool EsLineaValida(PedidoLinea linea)
    {
        return linea.Cantidad > 0
            && linea.PrecioUnitario >= 0
            && !string.IsNullOrWhiteSpace(linea.Producto)
            && !string.IsNullOrWhiteSpace(linea.Descripcion);
    }

    public static bool SonLineasValidas(IEnumerable<PedidoLinea> lineas)
    {
        var lista = lineas.ToList();
        return lista.Count > 0 && lista.All(EsLineaValida);
    }
}
