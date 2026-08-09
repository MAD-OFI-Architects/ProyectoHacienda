using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;

namespace Hacienda.Web.Controllers;

[Authorize]
public class VacunaController : Controller
{
    private readonly IServicioVacunacion _servicioVacunacion;
    private readonly IGestorPotreros _gestorPotreros;
    private readonly IGestorReses _gestorReses;

    public VacunaController(
        IServicioVacunacion servicioVacunacion,
        IGestorPotreros gestorPotreros,
        IGestorReses gestorReses)
    {
        _servicioVacunacion = servicioVacunacion;
        _gestorPotreros = gestorPotreros;
        _gestorReses = gestorReses;
    }

    public IActionResult Index()
    {
        var vacunas = _servicioVacunacion.ListarVacunasDisponibles();
        ViewBag.Estadisticas = _servicioVacunacion.ObtenerEstadisticas();
        return View(vacunas);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.TiposVacuna = new[] { "Bacteriana", "Viva" };
        ViewBag.Atenuaciones = Enum.GetValues<Viva.GradoAtenuacion>();
        return View();
    }

    [HttpPost]
    public IActionResult Create(string tipoVacuna, string nombre, string lote,
        DateTime fechaVencimiento, DateTime fechaAplicacion,
        uint? periodoAplicacion, Viva.GradoAtenuacion? atenuacion)
    {
        try
        {
            string mensaje;
            if (tipoVacuna == "Bacteriana")
            {
                if (!periodoAplicacion.HasValue)
                {
                    ViewBag.Mensaje = "El período de aplicación es requerido";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }
                mensaje = _servicioVacunacion.CrearVacunaBacteriana(nombre, lote, fechaVencimiento, fechaAplicacion, periodoAplicacion.Value);
            }
            else
            {
                if (!atenuacion.HasValue)
                {
                    ViewBag.Mensaje = "La atenuación es requerida";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }
                mensaje = _servicioVacunacion.CrearVacunaViva(nombre, lote, fechaVencimiento, fechaAplicacion, atenuacion.Value);
            }

            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ViewBag.Mensaje = ex.Message;
            ViewBag.TipoMensaje = "danger";
        }

        ViewBag.TiposVacuna = new[] { "Bacteriana", "Viva" };
        ViewBag.Atenuaciones = Enum.GetValues<Viva.GradoAtenuacion>();
        return View();
    }

    [HttpGet]
    public IActionResult CrearLote()
    {
        ViewBag.TiposVacuna = new[] { "Bacteriana", "Viva" };
        ViewBag.Atenuaciones = Enum.GetValues<Viva.GradoAtenuacion>();
        return View();
    }

    [HttpPost]
    public IActionResult CrearLote(string tipoVacuna, string nombre, string loteBase,
        DateTime fechaVencimiento, DateTime fechaAplicacion,
        uint? periodoAplicacion, Viva.GradoAtenuacion? atenuacion, uint cantidad)
    {
        try
        {
            string mensaje;
            if (tipoVacuna == "Bacteriana")
            {
                if (!periodoAplicacion.HasValue)
                {
                    ViewBag.Mensaje = "El período de aplicación es requerido";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }
                mensaje = _servicioVacunacion.CrearLoteVacunaBacteriana(nombre, loteBase, fechaVencimiento, fechaAplicacion, periodoAplicacion.Value, cantidad);
            }
            else
            {
                if (!atenuacion.HasValue)
                {
                    ViewBag.Mensaje = "La atenuación es requerida";
                    ViewBag.TipoMensaje = "danger";
                    return View();
                }
                mensaje = _servicioVacunacion.CrearLoteVacunaViva(nombre, loteBase, fechaVencimiento, fechaAplicacion, atenuacion.Value, cantidad);
            }

            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ViewBag.Mensaje = ex.Message;
            ViewBag.TipoMensaje = "danger";
        }

        ViewBag.TiposVacuna = new[] { "Bacteriana", "Viva" };
        ViewBag.Atenuaciones = Enum.GetValues<Viva.GradoAtenuacion>();
        return View();
    }

    [HttpGet]
    public IActionResult Aplicar()
    {
        ViewBag.Potreros = _gestorPotreros.ListarPotreros();
        ViewBag.Reses = _gestorReses.ListarReses();
        ViewBag.Vacunas = _servicioVacunacion.ListarVacunasDisponibles();
        return View();
    }

    [HttpPost]
    public IActionResult Aplicar(string potreroId, string nombreRes, string loteVacuna)
    {
        try
        {
            string mensaje = _servicioVacunacion.AplicarVacuna(loteVacuna, potreroId, nombreRes);
            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = mensaje.Contains("exito", StringComparison.OrdinalIgnoreCase) ? "success" : "danger";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ViewBag.Mensaje = ex.Message;
            ViewBag.TipoMensaje = "danger";
        }

        ViewBag.Potreros = _gestorPotreros.ListarPotreros();
        ViewBag.Reses = _gestorReses.ListarReses();
        ViewBag.Vacunas = _servicioVacunacion.ListarVacunasDisponibles();
        return View();
    }
}