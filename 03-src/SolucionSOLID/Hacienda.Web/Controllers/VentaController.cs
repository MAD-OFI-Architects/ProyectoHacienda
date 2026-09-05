using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hacienda.Application.Interfaces;

namespace Hacienda.Web.Controllers;

[Authorize]
public class VentaController : Controller
{
    private readonly IServicioVentas _servicioVentas;
    private readonly IGestorPotreros _gestorPotreros;

    public VentaController(IServicioVentas servicioVentas, IGestorPotreros gestorPotreros)
    {
        _servicioVentas = servicioVentas;
        _gestorPotreros = gestorPotreros;
    }

    public IActionResult Index()
    {
        var ventas = _servicioVentas.ListarVentas();
        ViewBag.Estadisticas = _servicioVentas.ObtenerEstadisticas();
        return View(ventas);
    }

    /// <summary>
    /// Inventario de productos derivados (SC-1: lácteos, carne, piel).
    /// </summary>
    public IActionResult Productos()
    {
        var productos = _servicioVentas.ListarProductosDerivados();
        return View(productos);
    }

    /// <summary>
    /// Formulario de venta de una res junto con productos derivados (SC-1).
    /// </summary>
    [HttpGet]
    public IActionResult VenderConDerivados()
    {
        CargarViewBags();
        return View(_servicioVentas.ListarProductosDerivados());
    }

    [HttpPost]
    public IActionResult VenderConDerivados(string potreroId, string nombreRes, decimal monto,
        IReadOnlyDictionary<string, int>? productos)
    {
        var seleccion = (productos ?? new Dictionary<string, int>())
            .Where(kv => kv.Value > 0)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        var resultado = _servicioVentas.VenderConDerivados(potreroId, nombreRes, monto, seleccion);

        TempData["Mensaje"] = resultado.Mensaje;
        TempData["TipoMensaje"] = resultado.Exito ? "success" : "danger";

        if (resultado.Exito)
            return RedirectToAction(nameof(Index));

        CargarViewBags();
        return View(_servicioVentas.ListarProductosDerivados());
    }

    private void CargarViewBags()
    {
        ViewBag.Potreros = _gestorPotreros.ListarPotreros();
    }
}
