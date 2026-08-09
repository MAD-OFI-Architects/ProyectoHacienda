using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;

namespace Hacienda.Application.Interfaces;

public interface IServicioVacunacion
{
    string AplicarVacuna(string loteVacuna, string potreroId, string nombreRes);
    string CrearVacunaBacteriana(string nombre, string lote,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo);
    string CrearVacunaViva(string nombre, string lote,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion);
    string CrearLoteVacunaBacteriana(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo, uint cantidad);
    string CrearLoteVacunaViva(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion, uint cantidad);
    List<Vacuna> ListarVacunasDisponibles();
    Dictionary<string, object> ObtenerEstadisticas();
}