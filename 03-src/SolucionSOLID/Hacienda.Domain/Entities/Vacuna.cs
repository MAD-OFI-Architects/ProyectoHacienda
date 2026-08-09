using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;

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
        Id = id;
        Nombre = nombre;
        Lote = lote;
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
        if (FechaVencimiento <= ahora.AddMonths(1))
            return EstadoVacuna.PorVencer;
        return EstadoVacuna.Vigente;
    }
}