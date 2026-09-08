using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories.Reses;

public class FabricaCebon : FabricaDeRes
{
    public FabricaCebon(IGuidProvider guidProvider) : base(guidProvider) { }

    public override TipoRes TipoAtendido => TipoRes.Cebon;
    public override TipoPotrero? TipoPotreroAtendido => TipoPotrero.Cebon;

    protected override Res Construir(Guid id, string nombre, uint peso, ushort edad)
        => new Cebon(id, nombre, peso, edad);
}
