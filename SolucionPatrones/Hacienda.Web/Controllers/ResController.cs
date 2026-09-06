using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hacienda.Application.Interfaces;
using Hacienda.Domain.Enums;

namespace Hacienda.Web.Controllers;

[Authorize]
public class ResController : Controller
{
    private readonly IGestorReses _gestorReses;
    private readonly IGestorPotreros _gestorPotreros;
    private readonly IServicioVentas _servicioVentas;

    public ResController(
        IGestorReses gestorReses,
        IGestorPotreros gestorPotreros,
        IServicioVentas servicioVentas)
    {
        _gestorReses = gestorReses;
        _gestorPotreros = gestorPotreros;
        _servicioVentas = servicioVentas;
    }

    public IActionResult Index()
    {
        var reses = _gestorReses.ListarReses();
        ViewBag.Estadisticas = _gestorReses.ObtenerEstadisticas();
        return View(reses);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Potreros = _gestorPotreros.ListarPotreros();
        ViewBag.TiposRes = Enum.GetValues<TipoRes>();
        return View();
    }

    [HttpPost]
    public IActionResult Create(string potreroId, string nombre, ushort edad, uint peso)
    {
        var resultado = _gestorReses.AgregarRes(potreroId, nombre, edad, peso);

        if (!resultado.Exito)
        {
            ViewBag.Mensaje = resultado.Mensaje;
            ViewBag.TipoMensaje = "danger";
            ViewBag.Potreros = _gestorPotreros.ListarPotreros();
            ViewBag.TiposRes = Enum.GetValues<TipoRes>();
            return View();
        }

        TempData["Mensaje"] = resultado.Mensaje;
        TempData["TipoMensaje"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Alimentar(string potreroId, string nombreRes, uint cantidadAlimento = 1)
    {
        var resultado = _gestorReses.AlimentarRes(potreroId, nombreRes, cantidadAlimento);

        TempData["Mensaje"] = resultado.Mensaje;
        TempData["TipoMensaje"] = resultado.Exito ? "success" : "danger";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Vender(string potreroId, string nombreRes, decimal monto)
    {
        var resultado = _servicioVentas.VenderRes(potreroId, nombreRes, monto);

        TempData["Mensaje"] = resultado.Mensaje;
        TempData["TipoMensaje"] = resultado.Exito ? "success" : "danger";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult DetalleVacunas(string potreroId, string nombreRes)
    {
        try
        {
            var potrero = _gestorPotreros.BuscarPotrero(potreroId);
            if (potrero == null)
            {
                TempData["Mensaje"] = $"Potrero '{potreroId}' no encontrado";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            var res = potrero.BuscarRes(nombreRes);
            if (res == null)
            {
                TempData["Mensaje"] = "Res no encontrada";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.PotreroId = potreroId;
            ViewBag.NombreRes = nombreRes;
            return View(res.VacunasAplicadas);
        }
        catch (Exception ex)
        {
            TempData["Mensaje"] = ex.Message;
            TempData["TipoMensaje"] = "danger";
            return RedirectToAction(nameof(Index));
        }
    }
}