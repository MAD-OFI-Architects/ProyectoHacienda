using Hacienda.Application.Results;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;

namespace Hacienda.Application.Interfaces;

public interface IGestorPotreros
{
    ResultadoOperacion CrearPotrero(string identificacion, TipoPotrero tipo);
    Potrero? BuscarPotrero(string identificacion);
    List<Potrero> ListarPotreros();
    Dictionary<string, object> ObtenerEstadisticas();
}