using Dapper;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace Hacienda.Infrastructure.Persistence.Sqlite;

public class RepositorioGeolocalizacionSqlite : IRepositorioGeolocalizacion
{
    private readonly string _connectionString;

    public RepositorioGeolocalizacionSqlite(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Geolocalizacion> ObtenerPorChipId(Guid chipId)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var rows = conn.Query<dynamic>(
            "SELECT * FROM geolocalizaciones WHERE chip_id = @ChipId ORDER BY fecha_hora DESC",
            new { ChipId = chipId.ToString() });

        var geolocalizaciones = new List<Geolocalizacion>();

        foreach (var row in rows)
        {
            geolocalizaciones.Add(MapearGeolocalizacion(row));
        }

        return geolocalizaciones;
    }

    public List<Geolocalizacion> ObtenerUltimas(int cantidad)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var rows = conn.Query<dynamic>(
            "SELECT * FROM geolocalizaciones ORDER BY fecha_hora DESC LIMIT @Cantidad",
            new { Cantidad = cantidad });

        var geolocalizaciones = new List<Geolocalizacion>();

        foreach (var row in rows)
        {
            geolocalizaciones.Add(MapearGeolocalizacion(row));
        }

        return geolocalizaciones;
    }

    public void Guardar(Geolocalizacion geolocalizacion)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        conn.Execute(
            @"INSERT INTO geolocalizaciones (id, chip_id, latitud, longitud, fecha_hora, precision_metros)
              VALUES (@Id, @ChipId, @Latitud, @Longitud, @FechaHora, @Precision)",
            new
            {
                Id = geolocalizacion.Id.ToString(),
                ChipId = geolocalizacion.ChipId.ToString(),
                Latitud = geolocalizacion.Latitud,
                Longitud = geolocalizacion.Longitud,
                FechaHora = geolocalizacion.FechaHora.ToString("yyyy-MM-dd HH:mm:ss"),
                Precision = geolocalizacion.PrecisionMetros
            });
    }

    public void GuardarTodas(List<Geolocalizacion> geolocalizaciones)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();

        conn.Execute("DELETE FROM geolocalizaciones", transaction: tx);

        foreach (var geo in geolocalizaciones)
        {
            conn.Execute(
                @"INSERT INTO geolocalizaciones (id, chip_id, latitud, longitud, fecha_hora, precision_metros)
                  VALUES (@Id, @ChipId, @Latitud, @Longitud, @FechaHora, @Precision)",
                new
                {
                    Id = geo.Id.ToString(),
                    ChipId = geo.ChipId.ToString(),
                    Latitud = geo.Latitud,
                    Longitud = geo.Longitud,
                    FechaHora = geo.FechaHora.ToString("yyyy-MM-dd HH:mm:ss"),
                    Precision = geo.PrecisionMetros
                },
                transaction: tx);
        }

        tx.Commit();
    }

    private static Geolocalizacion MapearGeolocalizacion(dynamic row)
    {
        var id = Guid.Parse((string)row.id);
        var chipId = Guid.Parse((string)row.chip_id);
        var latitud = (double)row.latitud;
        var longitud = (double)row.longitud;
        var fechaHora = DateTime.Parse((string)row.fecha_hora);
        double? precision = row.precision_metros == null ? null : (double)row.precision_metros;

        return new Geolocalizacion(id, chipId, latitud, longitud, fechaHora, precision);
    }
}
