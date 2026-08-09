using Hacienda.Domain.Enums;

namespace Hacienda.Domain.Entities;

public class Novillo : Res
{
    public Novillo(Guid id, string nombre, uint peso, ushort edad)
        : base(id, nombre, peso, edad) { }

    public override TipoRes Tipo => TipoRes.Novillo;
    public override byte MaxVacunasBacterianas => 2;
    public override byte MaxVacunasVivas => 2;
    public override ushort PesoMinimo => 400;
    public override ushort PesoRecomendadoVenta => 550;

    public override bool EsEdadValida(ushort edad) => edad > 48;

    public override string Serializar()
        => $"{Id}|{Nombre}|{Peso}|{Edad}|Novillo";
}