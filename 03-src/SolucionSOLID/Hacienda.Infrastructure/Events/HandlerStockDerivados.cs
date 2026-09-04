using Hacienda.Domain.Events;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Infrastructure.Events;

/// <summary>
/// Observer #2 (SC-1): reacciona a las ventas avisando derivados en o bajo el mínimo.
/// Se registra DESPUÉS de HandlerConsola — el orden lo garantiza el composition root.
/// </summary>
public class HandlerStockDerivados : ManejadorDeEventos<VentaRealizadaEvent>
{
    private readonly IRepositorioProducto _repoProductos;

    public HandlerStockDerivados(IRepositorioProducto repoProductos)
        => _repoProductos = repoProductos;

    public override void Manejar(VentaRealizadaEvent evento)
    {
        foreach (var producto in _repoProductos.ObtenerTodos().Where(p => p.EnStockMinimo))
            Console.WriteLine(
                $"[STOCK] El derivado '{producto.Nombre}' quedó en o bajo el mínimo ({producto.Stock} {producto.Unidad}). Reponer.");
    }
}
