using Dapper;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace Hacienda.Infrastructure.Persistence.Sqlite;

public class RepositorioProductoSqlite : IRepositorioProducto
{
    private readonly string _connectionString;
    private readonly IRegistroDeProductos _registroProductos;

    public RepositorioProductoSqlite(string connectionString, IRegistroDeProductos registroProductos)
    {
        _connectionString = connectionString;
        _registroProductos = registroProductos;
    }

    public List<ProductoDerivado> ObtenerTodos()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var rows = conn.Query<dynamic>("SELECT * FROM productos ORDER BY nombre");
        return rows.Select(MapearProducto).ToList();
    }

    public ProductoDerivado? ObtenerPorNombre(string nombre)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var row = conn.QueryFirstOrDefault<dynamic>(
            "SELECT * FROM productos WHERE nombre = @Nombre COLLATE NOCASE", new { Nombre = nombre });
        return row is null ? null : MapearProducto(row);
    }

    public ProductoDerivado? ObtenerPorId(Guid id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var row = conn.QueryFirstOrDefault<dynamic>(
            "SELECT * FROM productos WHERE id = @Id", new { Id = id.ToString() });
        return row is null ? null : MapearProducto(row);
    }

    public void Guardar(ProductoDerivado producto)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        conn.Execute(
            @"INSERT OR REPLACE INTO productos (id, nombre, tipo, precio, stock, stock_minimo)
              VALUES (@Id, @Nombre, @Tipo, @Precio, @Stock, @StockMinimo)",
            new
            {
                Id = producto.Id.ToString(),
                producto.Nombre,
                Tipo = (int)producto.Tipo,
                Precio = producto.Precio.Monto,
                Stock = (int)producto.Stock,
                StockMinimo = (int)producto.StockMinimo
            });
    }

    public void GuardarTodos(List<ProductoDerivado> productos)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();
        conn.Execute("DELETE FROM productos", transaction: tx);
        foreach (var p in productos)
            conn.Execute(
                @"INSERT INTO productos (id, nombre, tipo, precio, stock, stock_minimo)
                  VALUES (@Id, @Nombre, @Tipo, @Precio, @Stock, @StockMinimo)",
                new
                {
                    Id = p.Id.ToString(),
                    p.Nombre,
                    Tipo = (int)p.Tipo,
                    Precio = p.Precio.Monto,
                    Stock = (int)p.Stock,
                    StockMinimo = (int)p.StockMinimo
                }, transaction: tx);
        tx.Commit();
    }

    /// <summary>Rehidrata vía registro de fábricas — sin switches por tipo (P-01 idiomático).</summary>
    private ProductoDerivado MapearProducto(dynamic row)
    {
        var id = Guid.Parse((string)row.id);
        var tipo = (TipoProducto)(long)row.tipo;
        return _registroProductos.FabricaPara(tipo).Rehidratar(
            id,
            (string)row.nombre,
            new Domain.ValueObjects.Dinero((decimal)row.precio),
            (uint)(long)row.stock,
            (uint)(long)row.stock_minimo);
    }
}
