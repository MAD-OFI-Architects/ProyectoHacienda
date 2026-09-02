using Dapper;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace Hacienda.Infrastructure.Persistence.Sqlite;

public class RepositorioVacunaSqlite : IRepositorioVacuna
{
    private readonly string _connectionString;
    private readonly IGuidProvider _guidProvider;

    public RepositorioVacunaSqlite(string connectionString, IGuidProvider guidProvider)
    {
        _connectionString = connectionString;
        _guidProvider = guidProvider;
    }

    public List<Vacuna> ObtenerTodas()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var rows = conn.Query<dynamic>(
            @"SELECT v.* FROM vacunas v
              WHERE v.id NOT IN (SELECT vacuna_id FROM vacunas_aplicadas)");

        var vacunas = new List<Vacuna>();

        foreach (var row in rows)
        {
            vacunas.Add(MapearVacuna(row));
        }

        return vacunas;
    }

    public void GuardarTodas(List<Vacuna> vacunas)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();

        foreach (var vacuna in vacunas)
        {
            InsertVacuna(conn, tx, vacuna, actualizarSiExiste: true);
        }

        tx.Commit();
    }

    public void GuardarAplicadas(List<Potrero> potreros)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();

        foreach (var potrero in potreros)
            foreach (var res in potrero.Reses)
                foreach (var vacuna in res.VacunasAplicadas)
                {
                    conn.Execute(
                        "INSERT OR IGNORE INTO vacunas_aplicadas (res_id, vacuna_id) VALUES (@ResId, @VacunaId)",
                        new { ResId = res.Id.ToString(), VacunaId = vacuna.Id.ToString() },
                        transaction: tx);
                }

        tx.Commit();
    }

    public void CargarVacunasAplicadasEnPotreros(List<Potrero> potreros)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var rows = conn.Query<dynamic>(@"
            SELECT va.res_id, va.vacuna_id, v.*
            FROM vacunas_aplicadas va
            INNER JOIN vacunas v ON v.id = va.vacuna_id");

        foreach (var row in rows)
        {
            var resId = (string)row.res_id;
            var res = potreros.SelectMany(p => p.Reses)
                .FirstOrDefault(r => r.Id.ToString() == resId);

            if (res == null) continue;

            var vacuna = MapearVacuna(row);
            res.RegistrarVacunaEnHistorial(vacuna);
        }
    }

    internal static Vacuna MapearVacuna(dynamic row)
    {
        var id = Guid.Parse((string)row.id);
        var nombre = (string)row.nombre;
        var lote = (string)row.lote;
        var fechaVenc = DateTime.Parse((string)row.fecha_vencimiento);
        var fechaAplic = DateTime.Parse((string)row.fecha_aplicacion);
        var categoria = (string)row.categoria;

        if (categoria == "Bacteriana")
        {
            var periodo = (uint)(long)row.periodo_aplicacion;
            return new Bacteriana(id, nombre, lote, fechaVenc, fechaAplic, periodo);
        }

        if (categoria == "Viva")
        {
            var atenuacion = (Viva.GradoAtenuacion)(byte)(long)row.atenuacion;
            return new Viva(id, nombre, lote, fechaVenc, fechaAplic, atenuacion);
        }

        throw new InvalidOperationException($"Categoria de vacuna desconocida: {categoria}");
    }

    internal static void InsertVacuna(SqliteConnection conn, SqliteTransaction tx, Vacuna vacuna, bool actualizarSiExiste = false)
    {
        var sql = actualizarSiExiste
            ? @"INSERT OR REPLACE INTO vacunas (id, nombre, lote, fecha_vencimiento, fecha_aplicacion, categoria, periodo_aplicacion, atenuacion)
                VALUES (@Id, @Nombre, @Lote, @FechaVenc, @FechaAplic, @Categoria, @Periodo, @Atenuacion)"
            : @"INSERT INTO vacunas (id, nombre, lote, fecha_vencimiento, fecha_aplicacion, categoria, periodo_aplicacion, atenuacion)
                VALUES (@Id, @Nombre, @Lote, @FechaVenc, @FechaAplic, @Categoria, @Periodo, @Atenuacion)";

        if (vacuna is Bacteriana b)
        {
            conn.Execute(
                sql,
                new
                {
                    Id = b.Id.ToString(),
                    Nombre = b.Nombre,
                    Lote = b.Lote,
                    FechaVenc = b.FechaVencimiento.ToString("yyyy-MM-dd"),
                    FechaAplic = b.FechaAplicacion.ToString("yyyy-MM-dd"),
                    Categoria = "Bacteriana",
                    Periodo = (int)b.PeriodoAplicacion,
                    Atenuacion = (int?)null
                },
                transaction: tx);
        }
        else if (vacuna is Viva v)
        {
            conn.Execute(
                sql,
                new
                {
                    Id = v.Id.ToString(),
                    Nombre = v.Nombre,
                    Lote = v.Lote,
                    FechaVenc = v.FechaVencimiento.ToString("yyyy-MM-dd"),
                    FechaAplic = v.FechaAplicacion.ToString("yyyy-MM-dd"),
                    Categoria = "Viva",
                    Periodo = (int?)null,
                    Atenuacion = (int)(byte)v.Atenuacion
                },
                transaction: tx);
        }
        else
        {
            throw new InvalidOperationException($"Tipo de vacuna no soportado: {vacuna.GetType().Name}");
        }
    }
}
