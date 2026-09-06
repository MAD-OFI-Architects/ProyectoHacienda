using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories.Vacunas;

/// <summary>
/// Creator + Template Method (P-02, P-04, P-10, P-13): esqueleto sellado de vacunas y del LOTE común.
/// Los métodos gemelos CrearLoteVacunaBacteriana/Viva del servicio se consolidan aquí (una sola copia).
/// </summary>
public abstract class FabricaDeVacuna
{
    protected readonly IGuidProvider GuidProvider;

    protected FabricaDeVacuna(IGuidProvider guidProvider) => GuidProvider = guidProvider;

    public abstract VacunaCategoria CategoriaAtendida { get; }

    protected abstract Vacuna Construir(DatosVacuna datos);

    /// <summary>Reglas propias del subtipo: [] = puede construir (los Fallo del formulario viven aquí).</summary>
    public abstract IReadOnlyList<string> ValidarPropios(DatosVacuna datos);

    public Vacuna Crear(DatosVacuna datos)
    {
        ValidarComunes(datos);
        return Construir(datos);
    }

    /// <summary>Template Method del lote (P-10): numerado y salto de existentes viven UNA vez.</summary>
    public IEnumerable<(string Lote, Vacuna Vacuna)> CrearLote(
        DatosVacuna datosBase, uint cantidad, Func<string, bool> loteExiste)
    {
        if (cantidad == 0 || cantidad > 100)
            throw new ArgumentException("La cantidad debe estar entre 1 y 100");

        for (int i = 1; i <= cantidad; i++)
        {
            var loteNumerado = $"{datosBase.Lote}-{i:D3}";
            if (loteExiste(loteNumerado)) continue;
            var datos = datosBase with { Lote = loteNumerado };
            yield return (loteNumerado, Crear(datos));
        }
    }

    private static void ValidarComunes(DatosVacuna datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre))
            throw new ArgumentException("Nombre vacío", nameof(datos.Nombre));
        if (string.IsNullOrWhiteSpace(datos.Lote))
            throw new ArgumentException("Lote vacío", nameof(datos.Lote));
        if (datos.FechaVencimiento < datos.FechaAplicacion)
            throw new ArgumentException("El vencimiento no puede ser anterior a la aplicación");
    }
}
