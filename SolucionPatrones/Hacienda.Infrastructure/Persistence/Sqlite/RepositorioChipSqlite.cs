using Dapper;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.ValueObjects;
using Microsoft.Data.Sqlite;

namespace Hacienda.Infrastructure.Persistence.Sqlite;

public class RepositorioChipSqlite : IRepositorioChip
{
    private readonly string _connectionString;
    private readonly IGuidProvider _guidProvider;

    public RepositorioChipSqlite(string connectionString, IGuidProvider guidProvider)
    {
        _connectionString = connectionString;
        _guidProvider = guidProvider;
    }

    public List<Chip> ObtenerTodos()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var rows = conn.Query<dynamic>("SELECT * FROM chips");
        var chips = new List<Chip>();

        foreach (var row in rows)
        {
            var chip = MapearChip(row);
            chips.Add(chip);
        }

        return chips;
    }

    public Chip? ObtenerPorNumeroSerie(string numeroSerie)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var row = conn.QueryFirstOrDefault<dynamic>(
            "SELECT * FROM chips WHERE numero_serie = @NumeroSerie",
            new { NumeroSerie = numeroSerie });

        if (row == null) return null;

        return MapearChip(row);
    }

    public void Guardar(Chip chip)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        conn.Execute(
            @"INSERT OR REPLACE INTO chips (id, numero_serie, fecha_instalacion, estado)
              VALUES (@Id, @NumeroSerie, @FechaInstalacion, @Estado)",
            new
            {
                Id = chip.Id.ToString(),
                NumeroSerie = chip.NumeroSerie.Valor,
                FechaInstalacion = chip.FechaInstalacion.ToString("yyyy-MM-dd"),
                Estado = (byte)chip.Estado
            });
    }

    public void GuardarTodos(List<Chip> chips)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();

        conn.Execute("DELETE FROM chips", transaction: tx);

        foreach (var chip in chips)
        {
            conn.Execute(
                @"INSERT INTO chips (id, numero_serie, fecha_instalacion, estado)
                  VALUES (@Id, @NumeroSerie, @FechaInstalacion, @Estado)",
                new
                {
                    Id = chip.Id.ToString(),
                    NumeroSerie = chip.NumeroSerie.Valor,
                    FechaInstalacion = chip.FechaInstalacion.ToString("yyyy-MM-dd"),
                    Estado = (byte)chip.Estado
                },
                transaction: tx);
        }

        tx.Commit();
    }

    internal static Chip MapearChip(dynamic row)
    {
        var id = Guid.Parse((string)row.id);
        var numeroSerie = new NumeroSerieChip((string)row.numero_serie);
        var fechaInstalacion = DateTime.Parse((string)row.fecha_instalacion);
        var estado = (EstadoChip)(byte)(long)row.estado;

        var chip = new Chip(id, numeroSerie, fechaInstalacion);

        if (estado != EstadoChip.Activo)
        {
            chip.CambiarEstado(estado);
        }

        return chip;
    }
}
