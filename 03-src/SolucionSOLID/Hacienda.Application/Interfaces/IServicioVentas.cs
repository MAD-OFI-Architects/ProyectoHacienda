using Hacienda.Application.Results;
using Hacienda.Domain.Entities;

namespace Hacienda.Application.Interfaces;

public interface IServicioVentas
{
    ResultadoOperacion VenderRes(string potreroId, string nombreRes, decimal monto);
    ResultadoOperacion VenderConDerivados(string potreroId, string nombreRes, decimal monto,
        IReadOnlyDictionary<string, int> productos);
    List<Venta> ListarVentas();
    Dictionary<string, object> ObtenerEstadisticas();
}
