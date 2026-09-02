using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hacienda.Application.Interfaces;
using Hacienda.Application.Results;
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
            var resultado = tipoVacuna == "Bacteriana"
                ? (periodoAplicacion.HasValue
                    ? _servicioVacunacion.CrearVacunaBacteriana(nombre, lote, fechaVencimiento, fechaAplicacion, periodoAplicacion.Value)
                    : ResultadoOperacion.Fallo("El período de aplicación es requerido"))
                : (atenuacion.HasValue
                    ? _servicioVacunacion.CrearVacunaViva(nombre, lote, fechaVencimiento, fechaAplicacion, atenuacion.Value)
                    : ResultadoOperacion.Fallo("La atenuación es requerida"));

            if (!resultado.Exito)
            {
                ViewBag.Mensaje = resultado.Mensaje;
                ViewBag.TipoMensaje = "danger";
                ViewBag.TiposVacuna = new[] { "Bacteriana", "Viva" };
                ViewBag.Atenuaciones = Enum.GetValues<Viva.GradoAtenuacion>();
                return View();
            }

            TempData["Mensaje"] = resultado.Mensaje;
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
            ResultadoOperacion resultado;
            if (tipoVacuna == "Bacteriana")
            {
                if (!periodoAplicacion.HasValue)
                    return VistaFallida("El período de aplicación es requerido");

                resultado = _servicioVacunacion.CrearLoteVacunaBacteriana(nombre, loteBase, fechaVencimiento, fechaAplicacion, periodoAplicacion.Value, cantidad);
            }
            else
            {
                if (!atenuacion.HasValue)
                    return VistaFallida("La atenuación es requerida");

                resultado = _servicioVacunacion.CrearLoteVacunaViva(nombre, loteBase, fechaVencimiento, fechaAplicacion, atenuacion.Value, cantidad);
            }

            if (!resultado.Exito)
            {
                ViewBag.Mensaje = resultado.Mensaje;
                ViewBag.TipoMensaje = "danger";
                ViewBag.TiposVacuna = new[] { "Bacteriana", "Viva" };
                ViewBag.Atenuaciones = Enum.GetValues<Viva.GradoAtenuacion>();
                return View();
            }

            TempData["Mensaje"] = resultado.Mensaje;
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
            var resultado = _servicioVacunacion.AplicarVacuna(loteVacuna, potreroId, nombreRes);
            TempData["Mensaje"] = resultado.Mensaje;
            TempData["TipoMensaje"] = resultado.Exito ? "success" : "danger";
            return resultado.Exito ? RedirectToAction(nameof(Index)) : View();
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

    private IActionResult VistaFallida(string mensaje)
    {
        ViewBag.Mensaje = mensaje;
        ViewBag.TipoMensaje = "danger";
        ViewBag.TiposVacuna = new[] { "Bacteriana", "Viva" };
        ViewBag.Atenuaciones = Enum.GetValues<Viva.GradoAtenuacion>();
        return View();
    }
}