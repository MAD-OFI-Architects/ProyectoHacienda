using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Events;

public record VacunacionCompletadaEvent : IDomainEvent
{
    public string NombreRes { get; }
    public DateTime OcurridoEn { get; }

    public VacunacionCompletadaEvent(string nombreRes, DateTime ocurridoEn)
    {
        NombreRes = nombreRes;
        OcurridoEn = ocurridoEn;
    }
}

public record PesoMinimoEvent : IDomainEvent
{
    public string NombreRes { get; }
    public uint PesoActual { get; }
    public DateTime OcurridoEn { get; }

    public PesoMinimoEvent(string nombreRes, uint pesoActual, DateTime ocurridoEn)
    {
        NombreRes = nombreRes;
        PesoActual = pesoActual;
        OcurridoEn = ocurridoEn;
    }
}

public record PesoVentaEvent : IDomainEvent
{
    public string NombreRes { get; }
    public uint PesoActual { get; }
    public DateTime OcurridoEn { get; }

    public PesoVentaEvent(string nombreRes, uint pesoActual, DateTime ocurridoEn)
    {
        NombreRes = nombreRes;
        PesoActual = pesoActual;
        OcurridoEn = ocurridoEn;
    }
}

public record PotreroMitadEvent : IDomainEvent
{
    public string IdentificacionPotrero { get; }
    public DateTime OcurridoEn { get; }

    public PotreroMitadEvent(string identificacionPotrero, DateTime ocurridoEn)
    {
        IdentificacionPotrero = identificacionPotrero;
        OcurridoEn = ocurridoEn;
    }
}

public record PotreroLlenoEvent : IDomainEvent
{
    public string IdentificacionPotrero { get; }
    public DateTime OcurridoEn { get; }

    public PotreroLlenoEvent(string identificacionPotrero, DateTime ocurridoEn)
    {
        IdentificacionPotrero = identificacionPotrero;
        OcurridoEn = ocurridoEn;
    }
}

public record VacunaVencidaEvent : IDomainEvent
{
    public string NombreVacuna { get; }
    public string Lote { get; }
    public DateTime FechaVencimiento { get; }
    public DateTime OcurridoEn { get; }

    public VacunaVencidaEvent(string nombreVacuna, string lote, DateTime fechaVencimiento, DateTime ocurridoEn)
    {
        NombreVacuna = nombreVacuna;
        Lote = lote;
        FechaVencimiento = fechaVencimiento;
        OcurridoEn = ocurridoEn;
    }
}
public record VentaRealizadaEvent : IDomainEvent
{
    public string NombreRes { get; }
    public IReadOnlyList<string> Productos { get; }
    public decimal Total { get; }
    public DateTime OcurridoEn { get; }

    public VentaRealizadaEvent(string nombreRes, IReadOnlyList<string> productos, decimal total, DateTime ocurridoEn)
    {
        NombreRes = nombreRes;
        Productos = productos;
        Total = total;
        OcurridoEn = ocurridoEn;
    }
}
