using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Hacienda.Domain.Factories;

public class RegistroDeProductos : IRegistroDeProductos
{
    private readonly IReadOnlyDictionary<TipoProducto, FabricaDeProducto> _fabricas;

    public RegistroDeProductos(IEnumerable<FabricaDeProducto> fabricas)
        => _fabricas = fabricas.ToDictionary(f => f.TipoAtendido);

    public FabricaDeProducto FabricaPara(TipoProducto tipo)
    {
        if (!_fabricas.TryGetValue(tipo, out var fabrica))
            throw new ArgumentException($"Tipo de producto no soportado: {tipo}");
        return fabrica;
    }

    public ProductoDerivado Crear(TipoProducto tipo, string nombre, decimal precio, uint stock, uint stockMinimo)
        => FabricaPara(tipo).Crear(nombre, precio, stock, stockMinimo);
}
