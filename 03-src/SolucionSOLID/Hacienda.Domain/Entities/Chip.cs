using Hacienda.Domain.Enums;
using Hacienda.Domain.ValueObjects;
using System;

namespace Hacienda.Domain.Entities;

public class Chip : IChip
{
    public Guid Id { get; }
    public NumeroSerieChip NumeroSerie { get; }
    public DateTime FechaInstalacion { get; }
    public EstadoChip Estado { get; private set; }

    private Chip(Guid id, NumeroSerieChip numeroSerie, DateTime fechaInstalacion)
    {
        Id = id;
        NumeroSerie = numeroSerie;
        FechaInstalacion = fechaInstalacion;
        Estado = EstadoChip.Activo;
    }

    public static Chip Crear(Guid id, NumeroSerieChip numeroSerie, DateTime fechaInstalacion)
    {
        if (fechaInstalacion > DateTime.UtcNow.AddMinutes(1))
            throw new ArgumentException("La fecha de instalación no puede ser futura", nameof(fechaInstalacion));

        if (fechaInstalacion < new DateTime(2000, 1, 1))
            throw new ArgumentException("La fecha de instalación no puede ser anterior al año 2000", nameof(fechaInstalacion));

        return new Chip(id, numeroSerie, fechaInstalacion);
    }

    public void CambiarEstado(EstadoChip nuevoEstado)
    {
        if (!Enum.IsDefined(typeof(EstadoChip), nuevoEstado))
            throw new ArgumentException($"Estado de chip inválido: {nuevoEstado}", nameof(nuevoEstado));

        if (Estado == nuevoEstado)
            return;

        // Validaciones de transición de estado
        ValidarTransicionEstado(nuevoEstado);

        Estado = nuevoEstado;
    }

    private void ValidarTransicionEstado(EstadoChip nuevoEstado)
    {
        // Reglas de negocio para transiciones válidas
        switch (Estado)
        {
            case EstadoChip.Activo:
                if (nuevoEstado == EstadoChip.Perdido || nuevoEstado == EstadoChip.Dañado || nuevoEstado == EstadoChip.Inactivo)
                    return;
                break;
            case EstadoChip.Inactivo:
                if (nuevoEstado == EstadoChip.Activo || nuevoEstado == EstadoChip.Perdido || nuevoEstado == EstadoChip.Dañado)
                    return;
                break;
            case EstadoChip.Perdido:
            case EstadoChip.Dañado:
                // Estados terminales - solo pueden volver a Activo con autorización especial
                if (nuevoEstado == EstadoChip.Activo)
                    return;
                break;
        }

        throw new InvalidOperationException(
            $"Transición de estado no permitida: de {Estado} a {nuevoEstado}. " +
            "Contacte al administrador para transiciones especiales.");
    }

    public override string ToString() 
        => $"Chip: {NumeroSerie} | Estado: {Estado} | Instalado: {FechaInstalacion:yyyy-MM-dd}";
}