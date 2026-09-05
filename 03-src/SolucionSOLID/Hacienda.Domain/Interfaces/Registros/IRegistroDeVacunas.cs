using Hacienda.Domain.Entities;
using Hacienda.Domain.Factories.Reses;
using Hacienda.Domain.Factories.Vacunas;
using Hacienda.Domain.Factories.Productos;

namespace Hacienda.Domain.Interfaces;

/// <summary>Punto único de decisión de creación de vacunas por categoría (P-02).</summary>
public interface IRegistroDeVacunas
{
    IReadOnlyList<string> Validar(DatosVacuna datos);
    Vacuna Crear(DatosVacuna datos);
    FabricaDeVacuna FabricaPara(DatosVacuna datos);
}
