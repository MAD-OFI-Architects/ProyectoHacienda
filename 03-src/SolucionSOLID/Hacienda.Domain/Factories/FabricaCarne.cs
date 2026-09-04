using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.ValueObjects;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories;

public class FabricaCarne : FabricaDeProducto
{
    public FabricaCarne(IGuidProvider guidProvider) : base(guidProvider) { }

    public override TipoProducto TipoAtendido => TipoProducto.Carne;

    protected override ProductoDerivado Construir(Guid id, string nombre, Dinero precio, uint stock, uint stockMinimo)
        => new Carne(id, nombre, precio, stock, stockMinimo);
}
