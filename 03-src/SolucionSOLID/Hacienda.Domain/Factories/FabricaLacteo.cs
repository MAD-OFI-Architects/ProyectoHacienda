using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.ValueObjects;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories;

public class FabricaLacteo : FabricaDeProducto
{
    public FabricaLacteo(IGuidProvider guidProvider) : base(guidProvider) { }

    public override TipoProducto TipoAtendido => TipoProducto.Lacteo;

    protected override ProductoDerivado Construir(Guid id, string nombre, Dinero precio, uint stock, uint stockMinimo)
        => new Lacteo(id, nombre, precio, stock, stockMinimo);
}
