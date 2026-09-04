using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.ValueObjects;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories;

public class FabricaPiel : FabricaDeProducto
{
    public FabricaPiel(IGuidProvider guidProvider) : base(guidProvider) { }

    public override TipoProducto TipoAtendido => TipoProducto.Piel;

    protected override ProductoDerivado Construir(Guid id, string nombre, Dinero precio, uint stock, uint stockMinimo)
        => new Piel(id, nombre, precio, stock, stockMinimo);
}
