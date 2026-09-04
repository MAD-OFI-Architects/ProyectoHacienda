using Dapper;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.ValueObjects;
using Microsoft.Data.Sqlite;

namespace Hacienda.Infrastructure.Persistence.Sqlite;

public class RepositorioVentaSqlite : IRepositorioVenta
{
    private readonly string _connectionString;
    private readonly IGuidProvider _guidProvider;
    private readonly IRegistroDeReses _registroReses;

    public RepositorioVentaSqlite(string connectionString, IGuidProvider guidProvider, IRegistroDeReses registroReses)
    {
        _connectionString = connectionString;
        _guidProvider = guidProvider;
        _registroReses = registroReses;
    }

    public List<Venta> ObtenerTodas()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var rows = conn.Query<dynamic>("SELECT * FROM ventas");
        var itemRows = conn.Query<dynamic>("SELECT * FROM venta_items").ToLookup(r => (string)r.venta_id);
        var ventas = new List<Venta>();

        foreach (var row in rows)
        {
            var id = Guid.Parse((string)row.id);
            var fecha = DateTime.Parse((string)row.fecha);
            var resNombre = (string)row.res_nombre;
            var resPeso = (uint)(long)row.res_peso;
            var resEdad = (ushort)(long)row.res_edad;
            var resTipo = (string)row.res_tipo;
            var potreroOrigen = (string)row.potrero_origen;
            var monto = (decimal)row.monto;

            Res res = _registroReses.RehidratarDesdeTexto(_guidProvider.Nuevo(), resNombre, resPeso, resEdad, resTipo);

            var items = itemRows[id.ToString()]
                .Select(r => new VentaItem(
                    new ItemVendibleRegistro((string)r.descripcion),
                    (int)(long)r.cantidad,
                    (decimal)r.monto))
                .ToList();

            ventas.Add(new Venta(id, fecha, res, potreroOrigen, new Dinero(monto), items));
        }

        return ventas;
    }

    public void GuardarTodas(List<Venta> ventas)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var tx = conn.BeginTransaction();

        conn.Execute("DELETE FROM ventas", transaction: tx);
        conn.Execute("DELETE FROM venta_items", transaction: tx);

        foreach (var venta in ventas)
        {
            conn.Execute(
                @"INSERT INTO ventas (id, fecha, res_nombre, res_peso, res_edad, res_tipo, potrero_origen, monto)
                  VALUES (@Id, @Fecha, @ResNombre, @ResPeso, @ResEdad, @ResTipo, @PotreroOrigen, @Monto)",
                new
                {
                    Id = venta.Id.ToString(),
                    Fecha = venta.Fecha.ToString("yyyy-MM-dd"),
                    ResNombre = venta.Res.Nombre,
                    ResPeso = (int)venta.Res.Peso,
                    ResEdad = (int)venta.Res.Edad,
                    ResTipo = venta.Res.Tipo.ToString(),
                    PotreroOrigen = venta.PotreroOrigen,
                    Monto = venta.Monto.Monto
                },
                transaction: tx);

            foreach (var item in venta.Items)
                conn.Execute(
                    @"INSERT INTO venta_items (id, venta_id, descripcion, cantidad, monto)
                      VALUES (@Id, @VentaId, @Descripcion, @Cantidad, @Monto)",
                    new
                    {
                        Id = Guid.NewGuid().ToString(),
                        VentaId = venta.Id.ToString(),
                        Descripcion = item.Vendible.Descripcion,
                        item.Cantidad,
                        item.Monto
                    },
                    transaction: tx);
        }

        tx.Commit();
    }
}
