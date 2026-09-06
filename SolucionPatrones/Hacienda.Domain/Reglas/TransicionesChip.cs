using Hacienda.Domain.Enums;

namespace Hacienda.Domain.Reglas;

/// <summary>
/// Regla de negocio de transiciones de estado de un chip.
/// La tabla de transiciones es DATO declarativo; la regla (cómo se evalúa)
/// vive una sola vez aquí. Ni la entidad Chip ni el servicio tienen switches quemados.
/// </summary>
public static class TransicionesChip
{
    private static readonly IReadOnlyDictionary<EstadoChip, EstadoChip[]> Permitidas =
        new Dictionary<EstadoChip, EstadoChip[]>
        {
            [EstadoChip.Activo] = new[] { EstadoChip.Inactivo, EstadoChip.Perdido, EstadoChip.Dañado },
            [EstadoChip.Inactivo] = new[] { EstadoChip.Activo, EstadoChip.Perdido, EstadoChip.Dañado },
            [EstadoChip.Perdido] = new[] { EstadoChip.Activo },
            [EstadoChip.Dañado] = new[] { EstadoChip.Activo },
        };

    public static bool Permite(EstadoChip actual, EstadoChip nueva)
        => Permitidas.TryGetValue(actual, out var destinos) && destinos.Contains(nueva);
}
