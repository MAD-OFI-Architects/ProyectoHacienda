using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using System.Collections.Generic;

namespace Hacienda.Application.Interfaces;

public interface IServicioChip
{
    string InstalarChip(Guid resId, string numeroSerie);
    string CambiarEstadoChip(string numeroSerie, EstadoChip estado);
    IChip? ObtenerChipPorNumeroSerie(string numeroSerie);
    IChip? ObtenerChipPorResId(Guid resId);
    List<IChip> ListarChips();
}