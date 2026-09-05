using Hacienda.Application.Interfaces;
using Hacienda.Application.Results;
using Hacienda.Domain.Builders;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Events;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Application.Services;

public class ServicioVentas : IServicioVentas
{
    private readonly IRepositorioVenta _repoVenta;
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IRepositorioProducto _repoProducto;
    private readonly VentaBuilder _ventaBuilder;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly TimeProvider _reloj;

    public ServicioVentas(
        IRepositorioVenta repoVenta,
        IRepositorioPotrero repoPotrero,
        IRepositorioProducto repoProducto,
        VentaBuilder ventaBuilder,
        IDomainEventPublisher eventPublisher,
        TimeProvider reloj)
    {
        _repoVenta = repoVenta;
        _repoPotrero = repoPotrero;
        _repoProducto = repoProducto;
        _ventaBuilder = ventaBuilder;
        _eventPublisher = eventPublisher;
        _reloj = reloj;
    }

    public ResultadoOperacion VenderRes(string potreroId, string nombreRes, decimal monto)
    {
        var potreros = _repoPotrero.ObtenerTodos();
        var potrero = potreros.FirstOrDefault(p => p.Identificacion.Valor.Equals(potreroId, StringComparison.OrdinalIgnoreCase));
        if (potrero is null)
            return ResultadoOperacion.Fallo($"Potrero '{potreroId}' no encontrado");

        var res = potrero.BuscarRes(nombreRes);
        if (res is null)
            return ResultadoOperacion.Fallo($"Res '{nombreRes}' no encontrada");

        if (monto == 0)
            return ResultadoOperacion.Fallo("Monto debe ser mayor a 0");

        Venta venta = _ventaBuilder.Iniciar().ConRes(res, potreroId, monto).Build();

        potrero.RemoverRes(res);

        var ventas = _repoVenta.ObtenerTodas();
        ventas.Add(venta);
        _repoVenta.GuardarTodas(ventas);
        _repoPotrero.GuardarTodos(potreros);

        return ResultadoOperacion.Ok($"Venta de la res '{nombreRes}' realizada con éxito por {monto:C}.");
    }

    /// <summary>
    /// SC-1: venta multi-ítem — la res más N productos derivados en una sola operación.
    /// El builder ensambla y valida; el stock se descuenta en el dominio; el evento dispara
    /// a los handlers (consola primero, stock después).
    /// </summary>
    public ResultadoOperacion VenderConDerivados(string potreroId, string nombreRes, decimal monto,
        IReadOnlyDictionary<string, int> productos)
    {
        var potreros = _repoPotrero.ObtenerTodos();
        var potrero = potreros.FirstOrDefault(p => p.Identificacion.Valor.Equals(potreroId, StringComparison.OrdinalIgnoreCase));
        if (potrero is null)
            return ResultadoOperacion.Fallo($"Potrero '{potreroId}' no encontrado");

        var res = potrero.BuscarRes(nombreRes);
        if (res is null)
            return ResultadoOperacion.Fallo($"Res '{nombreRes}' no encontrada");

        if (monto == 0)
            return ResultadoOperacion.Fallo("Monto debe ser mayor a 0");

        var productosActuales = _repoProducto.ObtenerTodos();
        var builder = _ventaBuilder.Iniciar().ConRes(res, potreroId, monto);

        foreach (var (nombreProducto, cantidad) in productos)
        {
            var producto = productosActuales.FirstOrDefault(p =>
                p.Nombre.Equals(nombreProducto, StringComparison.OrdinalIgnoreCase));
            if (producto is null)
                return ResultadoOperacion.Fallo($"El producto '{nombreProducto}' no existe en el inventario");

            builder.ConProducto(producto, cantidad);
        }

        Venta venta = builder.Build();

        potrero.RemoverRes(res);

        var ventas = _repoVenta.ObtenerTodas();
        ventas.Add(venta);
        _repoVenta.GuardarTodas(ventas);
        _repoPotrero.GuardarTodos(potreros);
        _repoProducto.GuardarTodos(productosActuales);

        var nombres = venta.Items.Where(i => i.Vendible is ProductoDerivado)
            .Select(i => $"{i.Cantidad} {(i.Vendible as ProductoDerivado)!.Unidad} de {i.Vendible.Descripcion}").ToList();

        _eventPublisher.Publicar(new VentaRealizadaEvent(
            res.Nombre, nombres, venta.Monto.Monto, _reloj.GetUtcNow().DateTime));

        return ResultadoOperacion.Ok(
            $"Venta de la res '{nombreRes}' con {nombres.Count} derivado(s) realizada con éxito por {venta.Monto.Monto:C}.\n" +
            $"Ítems: {string.Join("; ", nombres)}");
    }

    public List<Venta> ListarVentas()
        => _repoVenta.ObtenerTodas().OrderByDescending(v => v.Fecha).ToList();

    public List<ProductoDerivado> ListarProductosDerivados()
        => _repoProducto.ObtenerTodos().OrderBy(p => p.Nombre).ToList();

    public Dictionary<string, object> ObtenerEstadisticas()
    {
        var ventas = _repoVenta.ObtenerTodas();
        var hoy = _reloj.GetUtcNow().DateTime;
        return new Dictionary<string, object>
        {
            ["TotalVentas"] = ventas.Count,
            ["MontoTotal"] = ventas.Sum(v => v.Monto.Monto),
            ["PromedioVenta"] = ventas.Any() ? ventas.Average(v => v.Monto.Monto) : 0,
            ["VentasEsteMes"] = ventas.Count(v => v.Fecha.Month == hoy.Month && v.Fecha.Year == hoy.Year),
            ["MontoEsteMes"] = ventas.Where(v => v.Fecha.Month == hoy.Month && v.Fecha.Year == hoy.Year).Sum(v => v.Monto.Monto)
        };
    }
}
