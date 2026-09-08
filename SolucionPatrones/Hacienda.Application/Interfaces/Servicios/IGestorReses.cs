using Hacienda.Application.Results;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;

namespace Hacienda.Application.Interfaces;

public interface IGestorReses
{
    ResultadoOperacion AgregarRes(string potreroId, string nombre, ushort edad, uint peso);
    ResultadoOperacion AlimentarRes(string potreroId, string nombreRes);
    ResultadoOperacion AlimentarRes(string potreroId, string nombreRes, uint cantidad);
    List<(Potrero Potrero, Res Res)> ListarReses();
    Dictionary<string, object> ObtenerEstadisticas();
}