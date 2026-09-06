using Hacienda.Domain.Enums;
using Hacienda.Domain.Reglas;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Domain.Entities;

public class Cebon : Res
{
    public Cebon(Guid id, string nombre, uint peso, ushort edad)
        : base(id, nombre, peso, edad) { }

    public override TipoRes Tipo => TipoRes.Cebon;
}