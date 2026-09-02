using Hacienda.Application.Results;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;

namespace Hacienda.Application.Interfaces;

public interface IServicioVacunacion
{
    ResultadoOperacion AplicarVacuna(string loteVacuna, string potreroId, string nombreRes);
    ResultadoOperacion CrearVacunaBacteriana(string nombre, string lote,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo);
    ResultadoOperacion CrearVacunaViva(string nombre, string lote,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion);
    ResultadoOperacion CrearLoteVacunaBacteriana(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo, uint cantidad);
    ResultadoOperacion CrearLoteVacunaViva(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion, uint cantidad);
    List<Vacuna> ListarVacunasDisponibles();
    Dictionary<string, object> ObtenerEstadisticas();
}