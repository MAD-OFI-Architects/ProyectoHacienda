using Hacienda.Domain.Entities;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories;

public interface IVentaFactory
{
    Venta Crear(Res res, string potreroOrigen, decimal monto, TimeProvider reloj);
}