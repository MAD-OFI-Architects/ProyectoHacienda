using Hacienda.Domain.Enums;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Hacienda.Domain.Factories.Vacunas;

public class RegistroDeVacunas : IRegistroDeVacunas
{
    private readonly IReadOnlyDictionary<VacunaCategoria, FabricaDeVacuna> _fabricas;

    public RegistroDeVacunas(IEnumerable<FabricaDeVacuna> fabricas)
        => _fabricas = fabricas.ToDictionary(f => f.CategoriaAtendida);

    public FabricaDeVacuna FabricaPara(DatosVacuna datos)
    {
        if (!_fabricas.TryGetValue(datos.Categoria, out var fabrica))
            throw new ArgumentException($"Categoría de vacuna no soportada: {datos.Categoria}");
        return fabrica;
    }

    public IReadOnlyList<string> Validar(DatosVacuna datos)
        => FabricaPara(datos).ValidarPropios(datos);

    public Vacuna Crear(DatosVacuna datos)
        => FabricaPara(datos).Crear(datos);
}
