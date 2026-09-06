using Hacienda.Domain.Entities;
using System.Collections.Generic;

namespace Hacienda.Application.Interfaces;

public interface IServicioGeolocalizacion
{
    string RegistrarUbicacion(string numeroSerieChip, double latitud, double longitud, double? precisionMetros = null);
    List<Geolocalizacion> ObtenerHistorialChip(string numeroSerieChip);
    List<Geolocalizacion> ObtenerUltimasUbicaciones(int cantidad = 10);
    List<Geolocalizacion> ObtenerUbicacionesCercanas(double latitud, double longitud, double radioKm = 1.0);
}