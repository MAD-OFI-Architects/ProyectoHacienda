using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Hacienda.Application.Services;

public class ServicioGeolocalizacion : IServicioGeolocalizacion
{
    private readonly IRepositorioGeolocalizacion _repoGeo;
    private readonly IRepositorioChip _repoChip;
    private readonly IGuidProvider _guidProvider;
    private readonly TimeProvider _reloj;

    public ServicioGeolocalizacion(
        IRepositorioGeolocalizacion repoGeo,
        IRepositorioChip repoChip,
        IGuidProvider guidProvider,
        TimeProvider reloj)
    {
        _repoGeo = repoGeo;
        _repoChip = repoChip;
        _guidProvider = guidProvider;
        _reloj = reloj;
    }

    public string RegistrarUbicacion(string numeroSerieChip, double latitud, double longitud, double? precisionMetros = null)
    {
        var chip = _repoChip.ObtenerPorNumeroSerie(numeroSerieChip);
        if (chip == null)
            return $"Chip con número de serie {numeroSerieChip} no encontrado";

        if (chip.Estado != EstadoChip.Activo)
            return $"El chip {numeroSerieChip} no está activo (estado: {chip.Estado})";

        if (latitud < -90 || latitud > 90)
            return "Latitud inválida (debe estar entre -90 y 90)";

        if (longitud < -180 || longitud > 180)
            return "Longitud inválida (debe estar entre -180 y 180)";

        var geo = new Geolocalizacion(
            _guidProvider.Nuevo(),
            chip.Id,
            latitud,
            longitud,
            _reloj.GetUtcNow().DateTime,
            precisionMetros
        );

        _repoGeo.Guardar(geo);

        return $"Ubicación registrada para chip {numeroSerieChip}: [{latitud}, {longitud}]";
    }

    public List<Geolocalizacion> ObtenerHistorialChip(string numeroSerieChip)
    {
        var chip = _repoChip.ObtenerPorNumeroSerie(numeroSerieChip);
        if (chip == null)
            return new List<Geolocalizacion>();

        return _repoGeo.ObtenerPorChipId(chip.Id);
    }

    public List<Geolocalizacion> ObtenerUltimasUbicaciones(int cantidad = 10)
    {
        return _repoGeo.ObtenerUltimas(cantidad);
    }

    public List<Geolocalizacion> ObtenerUbicacionesCercanas(double latitud, double longitud, double radioKm = 1.0)
    {
        // Implementación simplificada - en producción usar consulta geoespacial
        var todas = _repoGeo.ObtenerUltimas(1000);
        var cercanas = new List<Geolocalizacion>();

        foreach (var geo in todas)
        {
            var distancia = CalcularDistancia(latitud, longitud, geo.Latitud, geo.Longitud);
            if (distancia <= radioKm)
                cercanas.Add(geo);
        }

        return cercanas.OrderBy(g => CalcularDistancia(latitud, longitud, g.Latitud, g.Longitud)).ToList();
    }

    private static double CalcularDistancia(double lat1, double lon1, double lat2, double lon2)
    {
        // Fórmula de Haversine simplificada
        const double R = 6371; // Radio de la Tierra en km
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}