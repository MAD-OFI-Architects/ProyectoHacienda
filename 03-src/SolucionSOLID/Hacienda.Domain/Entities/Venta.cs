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
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador de la venta no puede ser vacío", nameof(id));

        if (res == null)
            throw new ArgumentNullException(nameof(res));

        if (string.IsNullOrWhiteSpace(potreroOrigen))
            throw new ArgumentException("El potrero de origen no puede estar vacío", nameof(potreroOrigen));

        if (monto == null)
            throw new ArgumentNullException(nameof(monto));

        Id = id;
        Fecha = fecha;
        Res = res;
        PotreroOrigen = potreroOrigen.Trim();
        Monto = monto;
    }
}