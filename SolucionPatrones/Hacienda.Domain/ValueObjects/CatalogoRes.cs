using Hacienda.Domain.Entities;
using Hacienda.Domain.Reglas;
using Hacienda.Domain.Enums;

namespace Hacienda.Domain.ValueObjects;

public sealed record ConfiguracionResTipo(
    TipoRes Tipo,
    string Nombre,
    RangoEdad RangoEdad,
    byte MaxVacunasBacterianas,
    byte MaxVacunasVivas,
    ushort PesoMinimo,
    ushort PesoRecomendadoVenta);

public static class CatalogoRes
{
    private static readonly IReadOnlyDictionary<TipoRes, ConfiguracionResTipo> Configuraciones =
        new Dictionary<TipoRes, ConfiguracionResTipo>
        {
            [TipoRes.Ternero] = Desde(TipoRes.Ternero, "Ternero", ParametrosRes.Ternero),
            [TipoRes.Cebon] = Desde(TipoRes.Cebon, "Cebon", ParametrosRes.Cebon),
            [TipoRes.Novillo] = Desde(TipoRes.Novillo, "Novillo", ParametrosRes.Novillo),
            [TipoRes.VacaLechera] = Desde(TipoRes.VacaLechera, "VacaLechera", ParametrosRes.VacaLechera)
        };

    private static ConfiguracionResTipo Desde(TipoRes tipo, string nombre, ParametrosSubtipo parametros)
        => new(tipo, nombre, parametros.RangoEdad, parametros.MaxVacunasBacterianas,
            parametros.MaxVacunasVivas, parametros.PesoMinimo, parametros.PesoRecomendadoVenta);

    public static ConfiguracionResTipo Obtener(TipoRes tipo)
    {
        if (!Configuraciones.TryGetValue(tipo, out var configuracion))
            throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de res no soportado");

        return configuracion;
    }

    public static TipoRes Parsear(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("El tipo de res no puede ser vacío", nameof(valor));

        if (Enum.TryParse<TipoRes>(valor.Trim(), true, out var tipo))
            return tipo;

        throw new InvalidOperationException($"Tipo de res desconocido: {valor}");
    }
}
