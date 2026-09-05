using Hacienda.Domain.Entities;

namespace Hacienda.Domain.Interfaces;

public interface IRepositorioRes
{
    List<Res> ObtenerTodas();
    void GuardarTodas(List<Potrero> potreros);
    void CargarResesEnPotreros(List<Potrero> potreros);
}