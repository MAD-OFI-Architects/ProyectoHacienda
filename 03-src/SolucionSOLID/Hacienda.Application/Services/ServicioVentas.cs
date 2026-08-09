using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.Results;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Application.Services;

public class ServicioVentas : IServicioVentas
{
    private readonly IRepositorioVenta _repoVenta;
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IVentaFactory _fabricaVenta;
    private readonly IValidarVenta _validador;
    private readonly TimeProvider _reloj;

    public ServicioVentas(
        IRepositorioVenta repoVenta,
        IRepositorioPotrero repoPotrero,
        IVentaFactory fabricaVenta,
        IValidarVenta validador,
        TimeProvider reloj)
    {
        _repoVenta = repoVenta;
        _repoPotrero = repoPotrero;
        _fabricaVenta = fabricaVenta;
        _validador = validador;
        _reloj = reloj;
    }

    public string VenderRes(string potreroId, string nombreRes, decimal monto)
    {
        var potreros = _repoPotrero.ObtenerTodos();
        var potrero = potreros.FirstOrDefault(p => p.Identificacion.Valor.Equals(potreroId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Potrero '{potreroId}' no encontrado");
        var res = potrero.BuscarRes(nombreRes)
            ?? throw new InvalidOperationException($"Res '{nombreRes}' no encontrada");

        Venta venta = _fabricaVenta.Crear(res, potreroId, monto, _reloj);

        var validacion = _validador.Validar(venta);
        if (!validacion.EsValido)
            return string.Join("; ", validacion.Errores);

        potrero.RemoverRes(res);

        var ventas = _repoVenta.ObtenerTodas();
        ventas.Add(venta);
        _repoVenta.GuardarTodas(ventas);
        _repoPotrero.GuardarTodos(potreros);

        return $"Venta de la res '{nombreRes}' realizada con éxito por {monto:C}.";
    }

    public List<Venta> ListarVentas()
        => _repoVenta.ObtenerTodas().OrderByDescending(v => v.Fecha).ToList();

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