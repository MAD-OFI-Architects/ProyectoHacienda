using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hacienda.Application.Interfaces;

namespace Hacienda.Web.Controllers;

[Authorize]
public class UsuarioController : Controller
{
    private readonly IServicioAutenticacion _servicioAutenticacion;
    private readonly IAutorizador _autorizador;

    public UsuarioController(
        IServicioAutenticacion servicioAutenticacion,
        IAutorizador autorizador)
    {
        _servicioAutenticacion = servicioAutenticacion;
        _autorizador = autorizador;
    }

    public IActionResult Index()
    {
        var usuarios = _servicioAutenticacion.ObtenerTodosLosUsuarios();
        ViewBag.Estadisticas = new Dictionary<string, object>
        {
            ["TotalUsuarios"] = usuarios.Count
        };
        return View(usuarios);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public IActionResult Create(string nombre, string contrasena, string confirmarContrasena)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(contrasena))
        {
            ViewBag.Mensaje = "Todos los campos son requeridos";
            ViewBag.TipoMensaje = "danger";
            return View();
        }

        if (contrasena != confirmarContrasena)
        {
            ViewBag.Mensaje = "Las contraseñas no coinciden";
            ViewBag.TipoMensaje = "danger";
            return View();
        }

        try
        {
            var (exitoso, mensaje) = _servicioAutenticacion.CrearUsuario(nombre, contrasena);

            if (exitoso)
            {
                TempData["Mensaje"] = mensaje;
                TempData["TipoMensaje"] = "success";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Mensaje = mensaje;
            ViewBag.TipoMensaje = "danger";
            return View();
        }
        catch (Exception ex)
        {
            ViewBag.Mensaje = ex.Message;
            ViewBag.TipoMensaje = "danger";
            return View();
        }
    }
}