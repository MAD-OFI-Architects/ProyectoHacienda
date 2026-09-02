using Hacienda.Application.Results;
using Hacienda.Domain.Entities;

namespace Hacienda.Application.Interfaces;

public interface IServicioVentas
{
    ResultadoOperacion VenderRes(string potreroId, string nombreRes, decimal monto);
    List<Venta> ListarVentas();
    Dictionary<string, object> ObtenerEstadisticas();
}