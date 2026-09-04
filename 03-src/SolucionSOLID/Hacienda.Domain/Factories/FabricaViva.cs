using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories;

public class FabricaViva : FabricaDeVacuna
{
    public FabricaViva(IGuidProvider guidProvider) : base(guidProvider) { }

    public override VacunaCategoria CategoriaAtendida => VacunaCategoria.Viva;

    protected override Vacuna Construir(DatosVacuna datos)
        => new Viva(GuidProvider.Nuevo(), datos.Nombre, datos.Lote,
            datos.FechaVencimiento, datos.FechaAplicacion, datos.Atenuacion!.Value);

    public override IReadOnlyList<string> ValidarPropios(DatosVacuna datos)
        => datos.Atenuacion.HasValue
            ? Array.Empty<string>()
            : new[] { "La atenuación es requerida" };
}
