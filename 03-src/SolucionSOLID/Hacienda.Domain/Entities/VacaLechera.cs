using Hacienda.Domain.Enums;

namespace Hacienda.Domain.Entities;

/// <summary>
/// Subtipo lechero (SC-1 · D-05 Variante A): producción propia de leche.
/// Todos sus parámetros resuelven por catálogo (ParametrosRes.VacaLechera):
/// 1 clase diminuta + 1 entrada de catálogo = subtipo nuevo completo.
/// </summary>
public class VacaLechera : Res
{
    public VacaLechera(Guid id, string nombre, uint peso, ushort edad)
        : base(id, nombre, peso, edad) { }

    public override TipoRes Tipo => TipoRes.VacaLechera;
}
