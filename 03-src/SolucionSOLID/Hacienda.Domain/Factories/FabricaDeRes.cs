using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories;

/// <summary>
/// Creator + Template Method (P-01, P-04, P-13): esqueleto sellado de creación de reses.
/// Validar comunes → construir (hook Factory Method) → exigir la regla de edad del subtipo.
/// El rango describible y el mapeo potrero→tipo son DATOS del creator (sin switches, P-12).
/// </summary>
public abstract class FabricaDeRes
{
    protected readonly IGuidProvider GuidProvider;

    protected FabricaDeRes(IGuidProvider guidProvider) => GuidProvider = guidProvider;

    public abstract TipoRes TipoAtendido { get; }
    public abstract TipoPotrero? TipoPotreroAtendido { get; }

    protected abstract Res Construir(Guid id, string nombre, uint peso, ushort edad);

    public Res Crear(string nombre, uint peso, ushort edad)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre no puede ser vacío", nameof(nombre));

        // Las reglas del subtipo (edad/peso/id) las exige el ctor de Res — una sola vez, con su dueño.
        return Construir(GuidProvider.Nuevo(), nombre, peso, edad);
    }

    /// <summary>Rehidrata desde persistencia conservando el Id persistido (sin reglas: la res ya existía).</summary>
    public Res Rehidratar(Guid id, string nombre, uint peso, ushort edad)
        => Construir(id, nombre, peso, edad);
}
