using Hacienda.Domain.Entities;

namespace Hacienda.Domain.Interfaces;

public interface IRepositorioChip
{
    List<IChip> ObtenerTodos();
    IChip? ObtenerPorNumeroSerie(string numeroSerie);
    void Guardar(IChip chip);
    void GuardarTodos(List<IChip> chips);
}