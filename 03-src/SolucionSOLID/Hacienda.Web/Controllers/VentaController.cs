using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hacienda.Application.Interfaces;

namespace Hacienda.Web.Controllers;

[Authorize]
public class VentaController : Controller
{
    private readonly IServicioVentas _servicioVentas;

    public VentaController(IServicioVentas servicioVentas)
    {
        _servicioVentas = servicioVentas;
    }

    public IActionResult Index()
    {
        var ventas = _servicioVentas.ListarVentas();
        ViewBag.Estadisticas = _servicioVentas.ObtenerEstadisticas();
        return View(ventas);
    }
}