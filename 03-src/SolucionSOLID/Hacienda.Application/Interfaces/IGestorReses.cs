using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;

namespace Hacienda.Application.Interfaces;

public interface IGestorReses
{
    string AgregarRes(string potreroId, string nombre, ushort edad, uint peso);
    string AlimentarRes(string potreroId, string nombreRes);
    string AlimentarRes(string potreroId, string nombreRes, uint cantidad);
    List<(Potrero Potrero, Res Res)> ListarReses();
    Dictionary<string, object> ObtenerEstadisticas();
}