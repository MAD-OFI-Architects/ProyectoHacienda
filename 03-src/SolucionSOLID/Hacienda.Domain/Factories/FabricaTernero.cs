using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories;

public class FabricaTernero : FabricaDeRes
{
    public FabricaTernero(IGuidProvider guidProvider) : base(guidProvider) { }

    public override TipoRes TipoAtendido => TipoRes.Ternero;
    public override TipoPotrero? TipoPotreroAtendido => TipoPotrero.Ternero;

    protected override Res Construir(Guid id, string nombre, uint peso, ushort edad)
        => new Ternero(id, nombre, peso, edad);
}
