using Dapper;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace Hacienda.Infrastructure.Persistence.Sqlite;

public class RepositorioResSqlite : IRepositorioRes
{
    private readonly string _connectionString;
    private readonly IGuidProvider _guidProvider;

    public RepositorioResSqlite(string connectionString, IGuidProvider guidProvider)
    {
        _connectionString = connectionString;
        _guidProvider = guidProvider;
    }

    public List<Res> ObtenerTodas()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var rows = conn.Query<dynamic>("SELECT * FROM reses");
        var reses = new List<Res>();

        foreach (var row in rows)
        {
            var res = RepositorioPotreroSqlite.MapearRes(row);
            RepositorioPotreroSqlite.CargarChipSiExiste(res, row, conn);
            reses.Add(res);
        }

        return reses;
    }

    public void GuardarTodas(List<Potrero> potreros)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();

        var resesActuales = potreros.SelectMany(p => p.Reses).Select(r => r.Id.ToString()).ToHashSet();
        var resesEnDb = conn.Query<string>("SELECT id FROM reses", transaction: tx).ToHashSet();
        var resesABorrar = resesEnDb.Except(resesActuales).ToList();

        foreach (var resId in resesABorrar)
        {
            conn.Execute("DELETE FROM vacunas_aplicadas WHERE res_id = @Id", new { Id = resId }, transaction: tx);
            conn.Execute("DELETE FROM reses WHERE id = @Id", new { Id = resId }, transaction: tx);
        }

        foreach (var potrero in potreros)
        {
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

    public void CargarResesEnPotreros(List<Potrero> potreros)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var rows = conn.Query<dynamic>("SELECT * FROM reses");

        foreach (var row in rows)
        {
            var potreroId = (string)row.potrero_id;
            var potrero = potreros.FirstOrDefault(p => p.Id.ToString() == potreroId);

            if (potrero == null)
            {
                potrero = potreros.FirstOrDefault(p =>
                    p.Identificacion.Valor.Equals(potreroId, StringComparison.OrdinalIgnoreCase));
            }

            if (potrero != null)
            {
                var res = RepositorioPotreroSqlite.MapearRes(row);
                RepositorioPotreroSqlite.CargarChipSiExiste(res, row, conn);
                potrero.AgregarRes(res);
            }
        }
    }
}
