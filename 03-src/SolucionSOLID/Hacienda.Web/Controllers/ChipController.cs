using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hacienda.Application.Interfaces;
using Hacienda.Domain.Enums;

namespace Hacienda.Web.Controllers;

[Authorize]
public class ChipController : Controller
{
    private readonly IServicioChip _servicioChip;
    private readonly IServicioGeolocalizacion _servicioGeo;
    private readonly IGestorReses _gestorReses;

    public ChipController(
        IServicioChip servicioChip,
        IServicioGeolocalizacion servicioGeo,
        IGestorReses gestorReses)
    {
        _servicioChip = servicioChip;
        _servicioGeo = servicioGeo;
        _gestorReses = gestorReses;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var chips = _servicioChip.ListarChips();
        ViewBag.UltimasUbicaciones = _servicioGeo.ObtenerUltimasUbicaciones(50);
        return View(chips);
    }

    [HttpGet]
    public IActionResult Instalar()
    {
        ViewBag.Reses = _gestorReses.ListarReses();
        return View();
    }

    [HttpPost]
    public IActionResult Instalar(Guid resId, string numeroSerie)
    {
        var mensaje = _servicioChip.InstalarChip(resId, numeroSerie);
        TempData["Mensaje"] = mensaje;
        TempData["TipoMensaje"] = mensaje.Contains("correctamente") ? "success" : "danger";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult CambiarEstado(string numeroSerie, EstadoChip estado)
    {
        var mensaje = _servicioChip.CambiarEstadoChip(numeroSerie, estado);
        TempData["Mensaje"] = mensaje;
        TempData["TipoMensaje"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult RegistrarUbicacion()
    {
        ViewBag.Chips = _servicioChip.ListarChips();
        return View();
    }

    [HttpPost]
    public IActionResult RegistrarUbicacion(string numeroSerieChip, double latitud, double longitud, double? precisionMetros)
    {
        var mensaje = _servicioGeo.RegistrarUbicacion(numeroSerieChip, latitud, longitud, precisionMetros);
        TempData["Mensaje"] = mensaje;
        TempData["TipoMensaje"] = mensaje.Contains("registrada") ? "success" : "danger";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Historial(string numeroSerie)
    {
        var chip = _servicioChip.ObtenerChipPorNumeroSerie(numeroSerie);
        if (chip == null)
        {
            TempData["Mensaje"] = $"Chip '{numeroSerie}' no encontrado";
            TempData["TipoMensaje"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Chip = chip;
        ViewBag.Historial = _servicioGeo.ObtenerHistorialChip(numeroSerie);
        return View();
    }
}
