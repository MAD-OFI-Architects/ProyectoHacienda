using Hacienda.Domain.Enums;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Domain.Entities;

public interface IChip
{
    Guid Id { get; }
    NumeroSerieChip NumeroSerie { get; }
    DateTime FechaInstalacion { get; }
    EstadoChip Estado { get; }
    void CambiarEstado(EstadoChip nuevoEstado);
}