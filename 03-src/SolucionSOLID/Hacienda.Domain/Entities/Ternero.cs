using Hacienda.Domain.Enums;

namespace Hacienda.Domain.Entities;

public class Ternero : Res
{
    public Ternero(Guid id, string nombre, uint peso, ushort edad)
        : base(id, nombre, peso, edad) { }

    public override TipoRes Tipo => TipoRes.Ternero;
    public override byte MaxVacunasBacterianas => 3;
    public override byte MaxVacunasVivas => 1;
    public override ushort PesoMinimo => 150;
    public override ushort PesoRecomendadoVenta => 250;

    public override bool EsEdadValida(ushort edad) => edad <= 12;

    public override string Serializar()
        => $"{Id}|{Nombre}|{Peso}|{Edad}|Ternero";
}