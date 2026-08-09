using Hacienda.Domain.Entities;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Interfaces;

public interface IServicioVentas
{
    string VenderRes(string potreroId, string nombreRes, decimal monto);
    List<Venta> ListarVentas();
    Dictionary<string, object> ObtenerEstadisticas();
}