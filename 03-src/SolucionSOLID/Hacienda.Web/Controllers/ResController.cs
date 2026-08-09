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
        try
        {
            string mensaje = _gestorReses.AgregarRes(potreroId, nombre, edad, peso);
            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ViewBag.Mensaje = ex.Message;
            ViewBag.TipoMensaje = "danger";
        }

        ViewBag.Potreros = _gestorPotreros.ListarPotreros();
        ViewBag.TiposRes = Enum.GetValues<TipoRes>();
        return View();
    }

    [HttpPost]
    public IActionResult Alimentar(string potreroId, string nombreRes, uint cantidadAlimento = 1)
    {
        try
        {
            string mensaje = _gestorReses.AlimentarRes(potreroId, nombreRes, cantidadAlimento);
            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "success";
        }
        catch (Exception ex)
        {
            TempData["Mensaje"] = ex.Message;
            TempData["TipoMensaje"] = "danger";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Vender(string potreroId, string nombreRes, decimal monto)
    {
        try
        {
            string mensaje = _servicioVentas.VenderRes(potreroId, nombreRes, monto);
            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "success";
        }
        catch (Exception ex)
        {
            TempData["Mensaje"] = ex.Message;
            TempData["TipoMensaje"] = "danger";
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult DetalleVacunas(string potreroId, string nombreRes)
    {
        try
        {
            var potrero = _gestorPotreros.BuscarPotrero(potreroId);
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