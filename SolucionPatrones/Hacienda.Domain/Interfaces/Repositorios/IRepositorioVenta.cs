using Hacienda.Domain.Entities;

namespace Hacienda.Domain.Interfaces;

public interface IRepositorioVenta
{
    List<Venta> ObtenerTodas();
    void GuardarTodas(List<Venta> ventas);
}