using Hacienda.Domain.Entities;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Domain.Factories;

public class FabricaVenta : IVentaFactory
{
    private readonly IGuidProvider _guidProvider;

    public FabricaVenta(IGuidProvider guidProvider)
    {
        _guidProvider = guidProvider;
    }

    public Venta Crear(Res res, string potreroOrigen, decimal monto, TimeProvider reloj)
    {
        if (res == null) throw new ArgumentNullException(nameof(res));
        if (monto < 0) throw new ArgumentException("Monto negativo", nameof(monto));

        return new Venta(
            _guidProvider.Nuevo(),
            reloj.GetUtcNow().DateTime,
            res,
            potreroOrigen,
            new Dinero(monto));
    }
}