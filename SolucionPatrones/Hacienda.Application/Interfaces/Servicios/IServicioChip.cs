using Hacienda.Application.Results;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using System.Collections.Generic;

namespace Hacienda.Application.Interfaces;

public interface IServicioChip
{
    ResultadoOperacion InstalarChip(Guid resId, string numeroSerie);
    ResultadoOperacion CambiarEstadoChip(string numeroSerie, EstadoChip estado);
    Chip? ObtenerChipPorNumeroSerie(string numeroSerie);
    Chip? ObtenerChipPorResId(Guid resId);
    List<Chip> ListarChips();
}