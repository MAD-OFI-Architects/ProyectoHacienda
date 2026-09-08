using Hacienda.Application.Results;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;

namespace Hacienda.Application.Interfaces;

public interface IInstaladorChip
{
    ResultadoOperacion Instalar(Guid resId, string numeroSerieStr);
    ResultadoOperacion CambiarEstado(string numeroSerie, EstadoChip estado);
}
