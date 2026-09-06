using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.ValueObjects;
using System.Collections.Generic;
using System.Linq;

namespace Hacienda.Domain.Factories.Reses;

/// <summary>
/// Registro alimentado por DI con todos los creators (IEnumerable&lt;FabricaDeRes&gt;):
/// la decisión de subtipo vive aquí una sola vez; agregar tipo = 1 creator + 1 registro.
/// </summary>
public class RegistroDeReses : IRegistroDeReses
{
    private readonly IReadOnlyDictionary<TipoRes, FabricaDeRes> _fabricas;

    public RegistroDeReses(IEnumerable<FabricaDeRes> fabricas)
        => _fabricas = fabricas.ToDictionary(f => f.TipoAtendido);

    public Res Crear(TipoRes tipo, string nombre, uint peso, ushort edad)
    {
        if (!_fabricas.TryGetValue(tipo, out var fabrica))
            throw new ArgumentException($"Tipo de res no soportado: {tipo}");
        return fabrica.Crear(nombre, peso, edad);
    }

    public Res RehidratarDesdeTexto(Guid id, string nombre, uint peso, ushort edad, string tipoTexto)
    {
        var tipo = CatalogoRes.Parsear(tipoTexto);
        if (!_fabricas.TryGetValue(tipo, out var fabrica))
            throw new InvalidOperationException($"Tipo de res no soportado: {tipoTexto}");
        return fabrica.Rehidratar(id, nombre, peso, edad);
    }

    public TipoRes MapearDesdePotrero(TipoPotrero tipoPotrero)
    {
        var fabrica = _fabricas.Values.FirstOrDefault(f => f.TipoPotreroAtendido == tipoPotrero);
        if (fabrica is null)
            throw new ArgumentException($"Tipo de potrero no reconocido: {tipoPotrero}");
        return fabrica.TipoAtendido;
    }
}
