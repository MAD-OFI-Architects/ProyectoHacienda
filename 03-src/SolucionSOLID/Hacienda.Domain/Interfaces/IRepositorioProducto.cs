using Hacienda.Domain.Entities;

namespace Hacienda.Domain.Interfaces;

public interface IRepositorioProducto
{
    List<ProductoDerivado> ObtenerTodos();
    ProductoDerivado? ObtenerPorNombre(string nombre);
    ProductoDerivado? ObtenerPorId(Guid id);
    void Guardar(ProductoDerivado producto);
    void GuardarTodos(List<ProductoDerivado> productos);
}
