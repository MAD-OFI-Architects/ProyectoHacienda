using Hacienda.Domain.Entities;

namespace Hacienda.Domain.Interfaces;

public interface IRepositorioGeolocalizacion
{
    List<Geolocalizacion> ObtenerPorChipId(Guid chipId);
    List<Geolocalizacion> ObtenerUltimas(int cantidad);
    void Guardar(Geolocalizacion geolocalizacion);
    void GuardarTodas(List<Geolocalizacion> geolocalizaciones);
}