using Hacienda.Domain.Entities;
using Hacienda.Domain.Factories.Reses;
using Hacienda.Domain.Factories.Vacunas;
using Hacienda.Domain.Factories.Productos;
using Hacienda.Domain.Enums;

namespace Hacienda.Domain.Interfaces;

/// <summary>Punto único de creación de productos derivados (SC-1).</summary>
public interface IRegistroDeProductos
{
    ProductoDerivado Crear(TipoProducto tipo, string nombre, decimal precio, uint stock, uint stockMinimo);
    FabricaDeProducto FabricaPara(TipoProducto tipo);
}
