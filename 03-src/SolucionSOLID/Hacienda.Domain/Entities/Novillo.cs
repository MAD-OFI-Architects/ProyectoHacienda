using Hacienda.Domain.Enums;
using Hacienda.Domain.Reglas;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Domain.Entities;

public class Novillo : Res
{
    public Novillo(Guid id, string nombre, uint peso, ushort edad)
        : base(id, nombre, peso, edad) { }

    public override TipoRes Tipo => TipoRes.Novillo;
}