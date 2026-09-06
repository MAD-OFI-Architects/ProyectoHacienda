namespace Hacienda.Domain.Reglas;

/// <summary>
/// Parámetros de negocio de las vacunas bacterianas, centralizados.
/// La regla vive una sola vez; Bacteriana solo referencia (OCP).
/// </summary>
public static class ParametrosVacuna
{
    public const uint PeriodoAplicacionMin = 2;
    public const uint PeriodoAplicacionMax = 4;

    /// <summary>Meses de anticipación con que una vacuna cuenta como «Por Vencer».</summary>
    public const int MesesAvisoPorVencer = 1;
}
