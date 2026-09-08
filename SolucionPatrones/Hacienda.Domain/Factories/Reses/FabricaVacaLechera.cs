using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories.Reses;

/// <summary>
/// Creator del subtipo lechero (SC-1, D-05 Variante A): 1 clase + 1 registro, cero ediciones (demostración OCP).
/// Sin potrero propio aún: TipoPotreroAtendido = null (el mapeo lo declara cada creator).
/// </summary>
public class FabricaVacaLechera : FabricaDeRes
{
    public FabricaVacaLechera(IGuidProvider guidProvider) : base(guidProvider) { }

    public override TipoRes TipoAtendido => TipoRes.VacaLechera;
    public override TipoPotrero? TipoPotreroAtendido => null;

    protected override Res Construir(Guid id, string nombre, uint peso, ushort edad)
        => new VacaLechera(id, nombre, peso, edad);
}
