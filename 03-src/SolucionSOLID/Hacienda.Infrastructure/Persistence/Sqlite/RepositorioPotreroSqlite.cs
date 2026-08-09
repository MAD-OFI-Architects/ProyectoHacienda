using Dapper;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.ValueObjects;
using Microsoft.Data.Sqlite;

namespace Hacienda.Infrastructure.Persistence.Sqlite;

public class RepositorioPotreroSqlite : IRepositorioPotrero
{
    private readonly string _connectionString;
    private readonly IGuidProvider _guidProvider;

    public RepositorioPotreroSqlite(string connectionString, IGuidProvider guidProvider)
    {
        _connectionString = connectionString;
        _guidProvider = guidProvider;
    }

    public List<Potrero> ObtenerTodos()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var potreros = new List<Potrero>();
        var potreroRows = conn.Query<dynamic>("SELECT * FROM potreros");

        foreach (var row in potreroRows)
        {
            var id = Guid.Parse((string)row.id);
            var identificacion = new Identificacion((string)row.identificacion);
            var tipo = (TipoPotrero)(byte)row.tipo;
            var potrero = new Potrero(id, identificacion, tipo);

            var resRows = conn.Query<dynamic>(
                "SELECT * FROM reses WHERE potrero_id = @PotreroId",
                new { PotreroId = row.id });

            foreach (var resRow in resRows)
            {
                var res = MapearRes(resRow);
                CargarChipSiExiste(res, resRow, conn);
                potrero.AgregarRes(res);
            }

            potreros.Add(potrero);
        }

        return potreros;
    }

    public Potrero? ObtenerPorIdentificacion(string identificacion)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var row = conn.QueryFirstOrDefault<dynamic>(
            "SELECT * FROM potreros WHERE identificacion = @Identificacion COLLATE NOCASE",
            new { Identificacion = identificacion });

        if (row == null) return null;

        var id = Guid.Parse((string)row.id);
        var idVo = new Identificacion((string)row.identificacion);
        var tipo = (TipoPotrero)(byte)row.tipo;
        var potrero = new Potrero(id, idVo, tipo);

        var resRows = conn.Query<dynamic>(
            "SELECT * FROM reses WHERE potrero_id = @PotreroId",
            new { PotreroId = row.id });

        foreach (var resRow in resRows)
        {
            var res = MapearRes(resRow);
            CargarChipSiExiste(res, resRow, conn);
            potrero.AgregarRes(res);
        }

        return potrero;
    }

    public void GuardarTodos(List<Potrero> potreros)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();

        var idsActuales = potreros.Select(p => p.Id.ToString()).ToHashSet();
        var resesActuales = potreros.SelectMany(p => p.Reses).Select(r => r.Id.ToString()).ToHashSet();

        var potrerosEnDb = conn.Query<string>("SELECT id FROM potreros", transaction: tx).ToHashSet();
        var resesEnDb = conn.Query<string>("SELECT id FROM reses", transaction: tx).ToHashSet();

        var potrerosABorrar = potrerosEnDb.Except(idsActuales).ToList();
        var resesABorrar = resesEnDb.Except(resesActuales).ToList();

        foreach (var resId in resesABorrar)
        {
            conn.Execute("DELETE FROM vacunas_aplicadas WHERE res_id = @Id", new { Id = resId }, transaction: tx);
            conn.Execute("DELETE FROM reses WHERE id = @Id", new { Id = resId }, transaction: tx);
        }

        foreach (var potreroId in potrerosABorrar)
        {
            conn.Execute("DELETE FROM potreros WHERE id = @Id", new { Id = potreroId }, transaction: tx);
        }

        foreach (var potrero in potreros)
        {
            conn.Execute(
                "INSERT OR REPLACE INTO potreros (id, identificacion, tipo) VALUES (@Id, @Identificacion, @Tipo)",
                new
                {
                    Id = potrero.Id.ToString(),
                    Identificacion = potrero.Identificacion.Valor,
                    Tipo = (byte)potrero.Tipo
                },
                transaction: tx);

            foreach (var res in potrero.Reses)
            {
                conn.Execute(
                    "INSERT OR REPLACE INTO reses (id, potrero_id, nombre, peso, edad, tipo, chip_id) VALUES (@Id, @PotreroId, @Nombre, @Peso, @Edad, @Tipo, @ChipId)",
                    new
                    {
                        Id = res.Id.ToString(),
                        PotreroId = potrero.Id.ToString(),
                        Nombre = res.Nombre,
                        Peso = (int)res.Peso,
                        Edad = (int)res.Edad,
                        Tipo = res.Tipo.ToString(),
                        ChipId = res.Chip?.Id.ToString()
                    },
                    transaction: tx);
            }
        }

        tx.Commit();
    }

    internal static Res MapearRes(dynamic row)
    {
        var id = Guid.Parse((string)row.id);
        var nombre = (string)row.nombre;
        var peso = (uint)(long)row.peso;
        var edad = (ushort)(long)row.edad;
        var tipo = (string)row.tipo;

        return tipo switch
        {
            "Ternero" => new Ternero(id, nombre, peso, edad),
            "Novillo" => new Novillo(id, nombre, peso, edad),
            "Cebon" => new Cebon(id, nombre, peso, edad),
            _ => throw new InvalidOperationException($"Tipo de res desconocido: {tipo}")
        };
    }

    internal static void CargarChipSiExiste(Res res, dynamic row, SqliteConnection conn)
    {
        object? chipIdValue = row.chip_id;
        if (chipIdValue == null || chipIdValue is DBNull) return;

        var chipId = (string)chipIdValue;
        var chipRow = conn.QueryFirstOrDefault<dynamic>(
            "SELECT * FROM chips WHERE id = @ChipId",
            new { ChipId = chipId });

        if (chipRow != null)
        {
            res.Chip = RepositorioChipSqlite.MapearChip(chipRow);
        }
    }
}
