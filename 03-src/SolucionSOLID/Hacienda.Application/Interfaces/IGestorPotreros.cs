using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Interfaces;

public interface IGestorPotreros
{
    string CrearPotrero(string identificacion, TipoPotrero tipo);
    Potrero? BuscarPotrero(string identificacion);
    List<Potrero> ListarPotreros();
    Dictionary<string, object> ObtenerEstadisticas();
}