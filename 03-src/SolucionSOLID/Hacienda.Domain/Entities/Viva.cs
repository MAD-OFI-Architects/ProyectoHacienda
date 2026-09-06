using Hacienda.Domain.Enums;

namespace Hacienda.Domain.Entities;

public class Viva : Vacuna
{
    public enum GradoAtenuacion : byte
    {
        Atenuacion10 = 10,
        Atenuacion20 = 20,
        Atenuacion30 = 30
    }

    public GradoAtenuacion Atenuacion { get; }

    public Viva(Guid id, string nombre, string lote,
        DateTime fechaVencimiento, DateTime fechaAplicacion, GradoAtenuacion atenuacion)
        : base(id, nombre, lote, fechaVencimiento, fechaAplicacion)
    {
        Atenuacion = atenuacion;
    }

    public override VacunaCategoria Categoria => VacunaCategoria.Viva;

    protected override string SerializarSufijo() => ((byte)Atenuacion).ToString();

    public override string DetalleVisual() => $"Atenuada";
}