using Hacienda.Domain.Enums;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Domain.Entities;

public class Piel : ProductoDerivado
{
    public Piel(Guid id, string nombre, Dinero precio, uint stock, uint stockMinimo)
        : base(id, nombre, precio, stock, stockMinimo) { }

    public override TipoProducto Tipo => TipoProducto.Piel;
    public override string Unidad => "unidades";
}
