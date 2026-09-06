using Hacienda.Domain.Enums;
using Hacienda.Domain.Reglas;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Domain.Entities;

/// <summary>
/// Entidad con invariantes de dominio: el chip valida su propia creación y transición.
/// La regla de negocio no vive en el servicio ni en un factory ad hoc.
/// </summary>
public class Chip
{
    private static readonly DateTime FechaMinimaValida = new(2000, 1, 1);

    public Guid Id { get; }
    public NumeroSerieChip NumeroSerie { get; }
    public DateTime FechaInstalacion { get; }
    public EstadoChip Estado { get; private set; }

    public Chip(Guid id, NumeroSerieChip numeroSerie, DateTime fechaInstalacion)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador del chip no puede ser vacío", nameof(id));

        if (fechaInstalacion > DateTime.UtcNow.AddMinutes(1))
            throw new ArgumentException("La fecha de instalación no puede ser futura", nameof(fechaInstalacion));

        if (fechaInstalacion < FechaMinimaValida)
            throw new ArgumentException("La fecha de instalación no puede ser anterior al año 2000", nameof(fechaInstalacion));

        Id = id;
        NumeroSerie = numeroSerie;
        FechaInstalacion = fechaInstalacion;
        Estado = EstadoChip.Activo;
    }

    /// <summary>
    /// Mutación encapsulada del estado: valida la transición dentro del propio agregado.
    /// </summary>
    public void CambiarEstado(EstadoChip nuevoEstado)
    {
        if (!Enum.IsDefined(typeof(EstadoChip), nuevoEstado))
            throw new ArgumentOutOfRangeException(nameof(nuevoEstado), nuevoEstado, "Estado de chip inválido");

        if (Estado != nuevoEstado && !TransicionesChip.Permite(Estado, nuevoEstado))
            throw new InvalidOperationException(
                $"Transición de estado no permitida: de {Estado} a {nuevoEstado}. " +
                "Contacte al administrador para transiciones especiales.");

        Estado = nuevoEstado;
    }

    public override string ToString()
        => $"Chip: {NumeroSerie} | Estado: {Estado} | Instalado: {FechaInstalacion:yyyy-MM-dd}";
}
