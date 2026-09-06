using System.Reflection;
using Dapper;
using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.ValueObjects;
using Microsoft.Data.Sqlite;

namespace Hacienda.Infrastructure.CrossCutting;

public class DataLoader : IDataSeeder
{
    private const string SeedResourceName = "Hacienda.Infrastructure.CrossCutting.seed_data.sql";

    private readonly IRepositorioUsuario _repoUsuario;
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IGuidProvider _guidProvider;
    private readonly IHasher _hasher;
    private readonly string _connectionString;

    public DataLoader(
        IRepositorioUsuario repoUsuario,
        IRepositorioPotrero repoPotrero,
        IGuidProvider guidProvider,
        IHasher hasher,
        string connectionString)
    {
        _repoUsuario = repoUsuario;
        _repoPotrero = repoPotrero;
        _guidProvider = guidProvider;
        _hasher = hasher;
        _connectionString = connectionString;
    }

    public async Task CargarDatosAsync()
    {
        SeedUsuarios();
        await SeedDesdeSqlAsync();
    }

    private void SeedUsuarios()
    {
        var usuarios = _repoUsuario.ObtenerTodos();
        if (usuarios.Any()) return;

        var adminHash = _hasher.Hashear("admin123");
        var empHash = _hasher.Hashear("emp456");
        var visHash = _hasher.Hashear("visit789");

        var nuevos = new List<Usuario>
        {
            new Usuario(_guidProvider.Nuevo(), "admin", new Credencial(adminHash), RolUsuario.Admin),
            new Usuario(_guidProvider.Nuevo(), "empleado", new Credencial(empHash), RolUsuario.Empleado),
            new Usuario(_guidProvider.Nuevo(), "visitante", new Credencial(visHash), RolUsuario.Visitante),
        };
        _repoUsuario.GuardarTodos(nuevos);
    }

    private async Task SeedDesdeSqlAsync()
    {
        // Idempotent: if any potrero already exists, the seed has already run.
        var potreros = _repoPotrero.ObtenerTodos();
        if (potreros.Any()) return;

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(SeedResourceName);
        if (stream == null)
        {
            throw new InvalidOperationException(
                $"No se encontró el recurso embebido '{SeedResourceName}'. " +
                "Verifique que seed_data.sql esté incluido como EmbeddedResource en el .csproj.");
        }

        string sqlText;
        using (var reader = new StreamReader(stream))
        {
            sqlText = reader.ReadToEnd();
        }

        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        // The seed file is plain INSERT/PRAGMA statements separated by ';'.
        // Strip SQL line comments first, then execute each non-empty statement
        // individually so the multi-statement string is handled deterministically
        // (avoids relying on driver multi-statement support).
        var statements = StripLineComments(sqlText)
            .Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var raw in statements)
        {
            var stmt = raw.Trim();
            if (stmt.Length == 0) continue;

            await conn.ExecuteAsync(stmt);
        }
    }

    private static string StripLineComments(string sql)
    {
        var builder = new System.Text.StringBuilder(sql.Length);
        foreach (var line in sql.Split('\n'))
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("--"))
            {
                builder.Append('\n');
                continue;
            }
            builder.Append(line).Append('\n');
        }
        return builder.ToString();
    }
}
