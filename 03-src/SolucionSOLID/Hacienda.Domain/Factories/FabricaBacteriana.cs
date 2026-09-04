using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories;

public class FabricaBacteriana : FabricaDeVacuna
{
    public FabricaBacteriana(IGuidProvider guidProvider) : base(guidProvider) { }

    public override VacunaCategoria CategoriaAtendida => VacunaCategoria.Bacteriana;

    protected override Vacuna Construir(DatosVacuna datos)
        => new Bacteriana(GuidProvider.Nuevo(), datos.Nombre, datos.Lote,
            datos.FechaVencimiento, datos.FechaAplicacion, datos.PeriodoAplicacion!.Value);

    public override IReadOnlyList<string> ValidarPropios(DatosVacuna datos)
        => datos.PeriodoAplicacion.HasValue
            ? Array.Empty<string>()
            : new[] { "El período de aplicación es requerido" };
}
