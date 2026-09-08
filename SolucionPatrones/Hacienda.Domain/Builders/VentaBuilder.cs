using Hacienda.Domain.Entities;
using Hacienda.Domain.ValueObjects;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Builders;

/// <summary>
/// Builder (P-03): la única puerta de construcción de ventas multi-ítem.
/// Iniciar → ConRes (obligatoria, superficie actual) → ConProducto ×N → Build (invariantes + total).
/// El reloj y el guid entran por ctor como todo el sistema (mata la firma rara de IVentaFactory, P-13).
/// </summary>
public class VentaBuilder
{
    private readonly IGuidProvider _guidProvider;
    private readonly TimeProvider _reloj;

    private Res? _res;
    private string _potreroOrigen = string.Empty;
    private readonly List<VentaItem> _items = new();

    public VentaBuilder(IGuidProvider guidProvider, TimeProvider reloj)
    {
        _guidProvider = guidProvider;
        _reloj = reloj;
    }

    public VentaBuilder Iniciar()
    {
        _res = null;
        _potreroOrigen = string.Empty;
        _items.Clear();
        return this;
    }

    public VentaBuilder ConRes(Res res, string potreroOrigen, decimal monto)
    {
        if (res is null) throw new ArgumentNullException(nameof(res));
        if (monto < 0) throw new ArgumentException("Monto negativo", nameof(monto));

        _res = res;
        _potreroOrigen = potreroOrigen;
        _items.Add(new VentaItem(res, 1, monto));
        return this;
    }

    public VentaBuilder ConProducto(ProductoDerivado producto, int cantidad)
    {
        if (producto is null) throw new ArgumentNullException(nameof(producto));
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser al menos 1", nameof(cantidad));

        producto.DescontarStock((uint)cantidad);
        _items.Add(new VentaItem(producto, cantidad, (decimal)(producto.Precio.Monto * cantidad)));
        return this;
    }

    public Venta Build()
    {
        if (_res is null)
            throw new InvalidOperationException("La venta requiere una res");

        var total = _items.Sum(i => i.Monto);
        return new Venta(
            _guidProvider.Nuevo(),
            _reloj.GetUtcNow().DateTime,
            _res,
            _potreroOrigen,
            new Dinero(total),
            _items.AsReadOnly());
    }
}
