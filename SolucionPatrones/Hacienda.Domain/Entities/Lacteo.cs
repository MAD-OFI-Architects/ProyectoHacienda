using Hacienda.Domain.Enums;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Domain.Entities;

public class Lacteo : ProductoDerivado
{
    public Lacteo(Guid id, string nombre, Dinero precio, uint stock, uint stockMinimo)
        : base(id, nombre, precio, stock, stockMinimo) { }

    public override TipoProducto Tipo => TipoProducto.Lacteo;
    public override string Unidad => "litros";
}
