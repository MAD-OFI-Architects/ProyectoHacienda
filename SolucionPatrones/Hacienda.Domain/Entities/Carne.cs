using Hacienda.Domain.Enums;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Domain.Entities;

public class Carne : ProductoDerivado
{
    public Carne(Guid id, string nombre, Dinero precio, uint stock, uint stockMinimo)
        : base(id, nombre, precio, stock, stockMinimo) { }

    public override TipoProducto Tipo => TipoProducto.Carne;
    public override string Unidad => "kg";
}
