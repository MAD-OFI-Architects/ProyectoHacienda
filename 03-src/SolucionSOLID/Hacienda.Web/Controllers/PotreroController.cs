using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hacienda.Application.Interfaces;
using Hacienda.Domain.Enums;

namespace Hacienda.Web.Controllers;

[Authorize]
public class PotreroController : Controller
{
    private readonly IGestorPotreros _gestorPotreros;

    public PotreroController(IGestorPotreros gestorPotreros)
    {
        _gestorPotreros = gestorPotreros;
    }

    public IActionResult Index()
    {
        var potreros = _gestorPotreros.ListarPotreros();
        ViewBag.Estadisticas = _gestorPotreros.ObtenerEstadisticas();
        return View(potreros);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public IActionResult Create(string identificacion, TipoPotrero tipo)
    {
        try
        {
            string mensaje = _gestorPotreros.CrearPotrero(identificacion, tipo);
            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ViewBag.Mensaje = ex.Message;
            ViewBag.TipoMensaje = "danger";
            return View();
        }
    }

    public IActionResult Details(string id)
    {
        var potrero = _gestorPotreros.BuscarPotrero(id);
        if (potrero == null)
        {
            TempData["Mensaje"] = "Potrero no encontrado";
            TempData["TipoMensaje"] = "danger";
            return RedirectToAction(nameof(Index));
        }
        return View(potrero);
    }
}