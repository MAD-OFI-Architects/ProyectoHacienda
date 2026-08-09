using Hacienda.Domain.Entities;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Domain.Entities;

public class Venta
{
    public Guid Id { get; }
    public DateTime Fecha { get; }
    public Res Res { get; }
    public string PotreroOrigen { get; }
    public Dinero Monto { get; }

    public Venta(Guid id, DateTime fecha, Res res, string potreroOrigen, Dinero monto)
    {
        Id = id;
        Fecha = fecha;
        Res = res;
        PotreroOrigen = potreroOrigen;
        Monto = monto;
    }
}