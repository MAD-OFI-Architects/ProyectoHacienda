using Hacienda.Domain.Enums;
using Hacienda.Domain.Reglas;

namespace Hacienda.Domain.Entities;

public class Bacteriana : Vacuna
{
    public uint PeriodoAplicacion { get; }

    public Bacteriana(Guid id, string nombre, string lote,
        DateTime fechaVencimiento, DateTime fechaAplicacion, uint periodoAplicacion)
        : base(id, nombre, lote, fechaVencimiento, fechaAplicacion)
    {
        if (periodoAplicacion < ParametrosVacuna.PeriodoAplicacionMin || periodoAplicacion > ParametrosVacuna.PeriodoAplicacionMax)
            throw new ArgumentException(
                $"Período debe estar entre 2 y 4 semanas. Recibido: {periodoAplicacion}");
        PeriodoAplicacion = periodoAplicacion;
    }

    public override VacunaCategoria Categoria => VacunaCategoria.Bacteriana;

    protected override string SerializarSufijo() => PeriodoAplicacion.ToString();

    public override string DetalleVisual() => $"{PeriodoAplicacion} sem.";
}