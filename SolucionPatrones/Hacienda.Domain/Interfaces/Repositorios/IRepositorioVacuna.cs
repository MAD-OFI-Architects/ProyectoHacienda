using Hacienda.Domain.Entities;

namespace Hacienda.Domain.Interfaces;

public interface IRepositorioVacuna
{
    List<Vacuna> ObtenerTodas();
    void GuardarTodas(List<Vacuna> vacunas);
    void GuardarAplicadas(List<Potrero> potreros);
    void CargarVacunasAplicadasEnPotreros(List<Potrero> potreros);
}