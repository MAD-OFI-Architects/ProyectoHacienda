using Microsoft.Data.Sqlite;

namespace Hacienda.Infrastructure.Persistence.Sqlite;

public static class DatabaseInitializer
{
    public static void Initialize(string connectionString)
    {
        using var conn = new SqliteConnection(connectionString);
        conn.Open();

        ExecuteNonQuery(conn, """
            CREATE TABLE IF NOT EXISTS potreros (
                id TEXT PRIMARY KEY,
                identificacion TEXT NOT NULL UNIQUE,
                tipo INTEGER NOT NULL
            );
            """);

        ExecuteNonQuery(conn, """
            CREATE TABLE IF NOT EXISTS reses (
                id TEXT PRIMARY KEY,
                potrero_id TEXT NOT NULL,
                nombre TEXT NOT NULL,
                peso INTEGER NOT NULL,
                edad INTEGER NOT NULL,
                tipo TEXT NOT NULL,
                chip_id TEXT,
                FOREIGN KEY (potrero_id) REFERENCES potreros(id),
                FOREIGN KEY (chip_id) REFERENCES chips(id)
            );
            """);

        // Mitigate existing DB files created before the chip_id column existed.
        // SQLite throws if the column already exists; swallow that single error.
        try
        {
            ExecuteNonQuery(conn, "ALTER TABLE reses ADD COLUMN chip_id TEXT;");
        }
        catch (SqliteException)
        {
            // Column already exists — expected on databases created after the fix.
        }

        ExecuteNonQuery(conn, """
            CREATE TABLE IF NOT EXISTS vacunas (
                id TEXT PRIMARY KEY,
                nombre TEXT NOT NULL,
                lote TEXT NOT NULL,
                fecha_vencimiento TEXT NOT NULL,
                fecha_aplicacion TEXT NOT NULL,
                categoria TEXT NOT NULL,
                periodo_aplicacion INTEGER,
                atenuacion INTEGER
            );
            """);

        ExecuteNonQuery(conn, """
            CREATE TABLE IF NOT EXISTS vacunas_aplicadas (
                res_id TEXT NOT NULL,
                vacuna_id TEXT NOT NULL,
                PRIMARY KEY (res_id, vacuna_id),
                FOREIGN KEY (res_id) REFERENCES reses(id),
                FOREIGN KEY (vacuna_id) REFERENCES vacunas(id)
            );
            """);

        ExecuteNonQuery(conn, """
            CREATE TABLE IF NOT EXISTS ventas (
                id TEXT PRIMARY KEY,
                fecha TEXT NOT NULL,
                res_nombre TEXT NOT NULL,
                res_peso INTEGER NOT NULL,
                res_edad INTEGER NOT NULL,
                res_tipo TEXT NOT NULL,
                potrero_origen TEXT NOT NULL,
                monto REAL NOT NULL
            );
            """);

        ExecuteNonQuery(conn, """
            CREATE TABLE IF NOT EXISTS usuarios (
                id TEXT PRIMARY KEY,
                nombre TEXT NOT NULL,
                password_hash TEXT NOT NULL,
                rol INTEGER NOT NULL
            );
            """);

        ExecuteNonQuery(conn, """
            CREATE TABLE IF NOT EXISTS chips (
                id TEXT PRIMARY KEY,
                numero_serie TEXT NOT NULL,
                fecha_instalacion TEXT NOT NULL,
                estado INTEGER NOT NULL
            );
            """);

        ExecuteNonQuery(conn, """
            CREATE TABLE IF NOT EXISTS geolocalizaciones (
                id TEXT PRIMARY KEY,
                chip_id TEXT NOT NULL,
                latitud REAL NOT NULL,
                longitud REAL NOT NULL,
                fecha_hora TEXT NOT NULL,
                precision_metros REAL,
                FOREIGN KEY (chip_id) REFERENCES chips(id)
            );
            """);
    }

    private static void ExecuteNonQuery(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
