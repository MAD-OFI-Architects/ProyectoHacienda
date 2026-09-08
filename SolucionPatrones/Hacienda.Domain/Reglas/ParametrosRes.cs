using Hacienda.Domain.Enums;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Domain.Reglas;

/// <summary>
/// Parámetros de negocio de cada subtipo de res, centralizados.
/// <para>Único lugar donde viven los valores: si la normativa cambia
/// (rangos de edad, límites de vacunas, pesos), se modifica SOLO aquí —
/// las entidades solo referencian, no tienen valores quemados (OCP).</para>
/// </summary>
public sealed record ParametrosSubtipo(
    RangoEdad RangoEdad,
    byte MaxVacunasBacterianas,
    byte MaxVacunasVivas,
    ushort PesoMinimo,
    ushort PesoRecomendadoVenta);

public static class ParametrosRes
{
    public static readonly ParametrosSubtipo Ternero =
        new(RangoEdad.Hasta(12), MaxVacunasBacterianas: 3, MaxVacunasVivas: 1, PesoMinimo: 150, PesoRecomendadoVenta: 250);

    public static readonly ParametrosSubtipo Cebon =
        new(RangoEdad.Entre(13, 48), MaxVacunasBacterianas: 1, MaxVacunasVivas: 4, PesoMinimo: 290, PesoRecomendadoVenta: 420);

    public static readonly ParametrosSubtipo Novillo =
        new(RangoEdad.Desde(49), MaxVacunasBacterianas: 2, MaxVacunasVivas: 2, PesoMinimo: 400, PesoRecomendadoVenta: 550);

    /// <summary>Subtipo lechero (SC-1 · D-05 Variante A): producción propia.</summary>
    public static readonly ParametrosSubtipo VacaLechera =
        new(RangoEdad.Entre(24, 180), MaxVacunasBacterianas: 2, MaxVacunasVivas: 2, PesoMinimo: 400, PesoRecomendadoVenta: 600);

    /// <summary>Nombre plural por tipo para estadísticas (P-01): dato declarativo, sin cases hardcodeados.</summary>
    public static readonly IReadOnlyDictionary<TipoRes, string> PluralPorTipo =
        new Dictionary<TipoRes, string>
        {
            [TipoRes.Ternero] = "Terneros",
            [TipoRes.Cebon] = "Cebones",
            [TipoRes.Novillo] = "Novillos",
            [TipoRes.VacaLechera] = "VacaLecheras"
        };

    /// <summary>Resuelve los parámetros por tipo (útil para fábricas/servicios/estadísticas).</summary>
    public static ParametrosSubtipo ObtenerPorTipo(TipoRes tipo) => tipo switch
    {
        TipoRes.Ternero => Ternero,
        TipoRes.Cebon => Cebon,
        TipoRes.Novillo => Novillo,
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de res no soportado")
    };
}
