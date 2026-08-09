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
        Id = id;
        ChipId = chipId;
        Latitud = latitud;
        Longitud = longitud;
        FechaHora = fechaHora;
        PrecisionMetros = precisionMetros;
    }
}