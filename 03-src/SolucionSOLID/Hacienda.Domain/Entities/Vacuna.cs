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
    public abstract string Serializar();
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