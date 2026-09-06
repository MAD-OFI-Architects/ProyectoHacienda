using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.Reglas;

namespace Hacienda.Domain.Entities;

public abstract class Vacuna
{
    public Guid Id { get; }
    public string Nombre { get; }
    public string Lote { get; }
    public DateTime FechaVencimiento { get; }
    public DateTime FechaAplicacion { get; }

    protected Vacuna(Guid id, string nombre, string lote, DateTime fechaVencimiento, DateTime fechaAplicacion)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador de la vacuna no puede ser vacío", nameof(id));

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la vacuna no puede estar vacío", nameof(nombre));

        if (string.IsNullOrWhiteSpace(lote))
            throw new ArgumentException("El lote de la vacuna no puede estar vacío", nameof(lote));

        if (fechaVencimiento < fechaAplicacion)
            throw new ArgumentException("La fecha de vencimiento no puede ser anterior a la fecha de aplicación", nameof(fechaVencimiento));

        Id = id;
        Nombre = nombre.Trim();
        Lote = lote.Trim();
        FechaVencimiento = fechaVencimiento;
        FechaAplicacion = fechaAplicacion;
    }

    public abstract VacunaCategoria Categoria { get; }

    /// <summary>
    /// Serialización pipe de la vacuna (P-09): formato ÚNICO en la base.
    /// El template method sella el marco común (Nombre|Lote|fechas|Categoría) y solo
    /// el sufijo —que difiere en tipo de dato por subtipo— lo aporta el hook.
    /// </summary>
    public string Serializar()
        => $"{Nombre}|{Lote}|{FechaVencimiento:yyyy-MM-dd}|{FechaAplicacion:yyyy-MM-dd}|{Categoria}|{SerializarSufijo()}";

    /// <summary>Hook (Factory Method): el dato propio del subtipo al final de la serialización.</summary>
    protected abstract string SerializarSufijo();

    public abstract string DetalleVisual();

    public EstadoVacuna CalcularEstado(TimeProvider reloj)
    {
        var ahora = reloj.GetUtcNow();
        if (FechaVencimiento <= ahora)
            return EstadoVacuna.Vencida;
        if (FechaVencimiento <= ahora.AddMonths(ParametrosVacuna.MesesAvisoPorVencer))
            return EstadoVacuna.PorVencer;
        return EstadoVacuna.Vigente;
    }
}