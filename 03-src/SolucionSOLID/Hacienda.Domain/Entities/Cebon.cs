using Hacienda.Domain.Enums;

namespace Hacienda.Domain.Entities;

public class Cebon : Res
{
    public Cebon(Guid id, string nombre, uint peso, ushort edad)
        : base(id, nombre, peso, edad) { }

    public override TipoRes Tipo => TipoRes.Cebon;
    public override byte MaxVacunasBacterianas => 1;
    public override byte MaxVacunasVivas => 4;
    public override ushort PesoMinimo => 290;
    public override ushort PesoRecomendadoVenta => 420;

    public override bool EsEdadValida(ushort edad) => edad > 12 && edad <= 48;

    public override string Serializar()
        => $"{Id}|{Nombre}|{Peso}|{Edad}|Cebon";
}