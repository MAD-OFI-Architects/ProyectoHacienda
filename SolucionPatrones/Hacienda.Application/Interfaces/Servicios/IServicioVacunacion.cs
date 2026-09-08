using Hacienda.Application.Results;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Factories.Reses;
using Hacienda.Domain.Factories.Vacunas;
using Hacienda.Domain.Factories.Productos;

namespace Hacienda.Application.Interfaces;

public interface IServicioVacunacion
{
    ResultadoOperacion CrearVacuna(DatosVacuna datos);
    ResultadoOperacion CrearLoteVacunaBacteriana(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo, uint cantidad);
    ResultadoOperacion CrearLoteVacunaViva(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion, uint cantidad);
    ResultadoOperacion AplicarVacuna(string loteVacuna, string potreroId, string nombreRes);
    List<Vacuna> ListarVacunasDisponibles();
    Dictionary<string, object> ObtenerEstadisticas();
}
