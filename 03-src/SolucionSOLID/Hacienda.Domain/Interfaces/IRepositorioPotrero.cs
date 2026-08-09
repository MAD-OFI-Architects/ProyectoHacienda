using Hacienda.Domain.Entities;

namespace Hacienda.Domain.Interfaces;

public interface IRepositorioPotrero
{
    List<Potrero> ObtenerTodos();
    Potrero? ObtenerPorIdentificacion(string identificacion);
    void GuardarTodos(List<Potrero> potreros);
}