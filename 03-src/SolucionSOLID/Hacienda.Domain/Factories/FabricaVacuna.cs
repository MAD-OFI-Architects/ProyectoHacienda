using Hacienda.Domain.Enums;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories;

public class FabricaVacuna : IVacunaFactory
{
    private readonly IGuidProvider _guidProvider;

    public FabricaVacuna(IGuidProvider guidProvider)
    {
        _guidProvider = guidProvider;
    }

    public Vacuna CrearBacteriana(string nombre, string lote,
        DateTime fechaVencimiento, DateTime fechaAplicacion, uint periodoAplicacion)
    {
        ValidarParametrosComunes(nombre, lote, fechaVencimiento, fechaAplicacion);
        return new Bacteriana(_guidProvider.Nuevo(), nombre, lote,
            fechaVencimiento, fechaAplicacion, periodoAplicacion);
    }

    public Vacuna CrearViva(string nombre, string lote,
        DateTime fechaVencimiento, DateTime fechaAplicacion, Viva.GradoAtenuacion atenuacion)
    {
        ValidarParametrosComunes(nombre, lote, fechaVencimiento, fechaAplicacion);
        return new Viva(_guidProvider.Nuevo(), nombre, lote,
            fechaVencimiento, fechaAplicacion, atenuacion);
    }

    private static void ValidarParametrosComunes(
        string nombre, string lote, DateTime fechaVenc, DateTime fechaAplic)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("Nombre vacío", nameof(nombre));
        if (string.IsNullOrWhiteSpace(lote))
            throw new ArgumentException("Lote vacío", nameof(lote));
        if (fechaVenc < fechaAplic)
            throw new ArgumentException("El vencimiento no puede ser anterior a la aplicación");
    }
}