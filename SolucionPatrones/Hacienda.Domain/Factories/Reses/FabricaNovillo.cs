using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories.Reses;

public class FabricaNovillo : FabricaDeRes
{
    public FabricaNovillo(IGuidProvider guidProvider) : base(guidProvider) { }

    public override TipoRes TipoAtendido => TipoRes.Novillo;
    public override TipoPotrero? TipoPotreroAtendido => TipoPotrero.Novillo;

    protected override Res Construir(Guid id, string nombre, uint peso, ushort edad)
        => new Novillo(id, nombre, peso, edad);
}
