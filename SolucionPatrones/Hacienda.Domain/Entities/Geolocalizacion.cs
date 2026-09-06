using System;

namespace Hacienda.Domain.Entities;

public class Geolocalizacion
{
    public Guid Id { get; }
    public Guid ChipId { get; }
    public double Latitud { get; }
    public double Longitud { get; }
    public DateTime FechaHora { get; }
    public double? PrecisionMetros { get; }

    public Geolocalizacion(Guid id, Guid chipId, double latitud, double longitud, DateTime fechaHora, double? precisionMetros = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador de la geolocalización no puede ser vacío", nameof(id));

        if (chipId == Guid.Empty)
            throw new ArgumentException("El identificador del chip no puede ser vacío", nameof(chipId));

        if (!EsLatitudValida(latitud))
            throw new ArgumentOutOfRangeException(nameof(latitud), latitud, "La latitud debe estar entre -90 y 90");

        if (!EsLongitudValida(longitud))
            throw new ArgumentOutOfRangeException(nameof(longitud), longitud, "La longitud debe estar entre -180 y 180");

        if (precisionMetros is < 0)
            throw new ArgumentOutOfRangeException(nameof(precisionMetros), precisionMetros, "La precisión no puede ser negativa");

        Id = id;
        ChipId = chipId;
        Latitud = latitud;
        Longitud = longitud;
        FechaHora = fechaHora;
        PrecisionMetros = precisionMetros;
    }

    /// <summary>Regla de rango de coordenadas: UNA sola definición (la entidad es la dueña).</summary>
    public static bool EsLatitudValida(double latitud) => latitud >= -90 && latitud <= 90;

    public static bool EsLongitudValida(double longitud) => longitud >= -180 && longitud <= 180;

}