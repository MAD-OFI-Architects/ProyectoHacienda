using Hacienda.Domain.Entities;

namespace Hacienda.Domain.Interfaces;

public interface IRepositorioChip
{
    List<Chip> ObtenerTodos();
    Chip? ObtenerPorNumeroSerie(string numeroSerie);
    void Guardar(Chip chip);
    void GuardarTodos(List<Chip> chips);
}