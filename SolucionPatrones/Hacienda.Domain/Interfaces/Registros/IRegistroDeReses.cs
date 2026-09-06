using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;

namespace Hacienda.Domain.Interfaces;

/// <summary>Punto único de decisión de creación/rehidratación de reses (P-01).</summary>
public interface IRegistroDeReses
{
    Res Crear(TipoRes tipo, string nombre, uint peso, ushort edad);
    Res RehidratarDesdeTexto(Guid id, string nombre, uint peso, ushort edad, string tipoTexto);
    TipoRes MapearDesdePotrero(TipoPotrero tipoPotrero);
}
