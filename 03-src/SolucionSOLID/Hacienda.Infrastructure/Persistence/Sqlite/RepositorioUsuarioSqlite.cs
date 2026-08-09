using Dapper;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.ValueObjects;
using Microsoft.Data.Sqlite;

namespace Hacienda.Infrastructure.Persistence.Sqlite;

public class RepositorioUsuarioSqlite : IRepositorioUsuario
{
    private readonly string _connectionString;
    private readonly IGuidProvider _guidProvider;

    public RepositorioUsuarioSqlite(string connectionString, IGuidProvider guidProvider)
    {
        _connectionString = connectionString;
        _guidProvider = guidProvider;
    }

    public List<Usuario> ObtenerTodos()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var rows = conn.Query<dynamic>("SELECT * FROM usuarios");
        var usuarios = new List<Usuario>();

        foreach (var row in rows)
        {
            var id = Guid.Parse((string)row.id);
            var nombre = (string)row.nombre;
            var passwordHash = (string)row.password_hash;
            var rol = (RolUsuario)(byte)(long)row.rol;

            usuarios.Add(new Usuario(id, nombre, new Credencial(passwordHash), rol));
        }

        return usuarios;
    }

    public void GuardarTodos(List<Usuario> usuarios)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();

        conn.Execute("DELETE FROM usuarios", transaction: tx);

        foreach (var usuario in usuarios)
        {
            conn.Execute(
                "INSERT INTO usuarios (id, nombre, password_hash, rol) VALUES (@Id, @Nombre, @PasswordHash, @Rol)",
                new
                {
                    Id = usuario.Id.ToString(),
                    Nombre = usuario.Nombre,
                    PasswordHash = usuario.Credencial.PasswordHash,
                    Rol = (byte)usuario.Rol
                },
                transaction: tx);
        }

        tx.Commit();
    }
}
