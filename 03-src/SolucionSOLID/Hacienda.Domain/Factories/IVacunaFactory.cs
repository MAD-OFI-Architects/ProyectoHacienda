using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;

namespace Hacienda.Domain.Factories;

public interface IVacunaFactory
{
    Vacuna CrearBacteriana(string nombre, string lote,
        DateTime fechaVencimiento, DateTime fechaAplicacion, uint periodoAplicacion);

    Vacuna CrearViva(string nombre, string lote,
        DateTime fechaVencimiento, DateTime fechaAplicacion, Viva.GradoAtenuacion atenuacion);
}