using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Hacienda.Application.Interfaces;
using Hacienda.Web.Models;

namespace Hacienda.Web.Controllers;

public class AccountController : Controller
{
    private readonly IServicioAutenticacion _servicioAutenticacion;

    public AccountController(IServicioAutenticacion servicioAutenticacion)
    {
        _servicioAutenticacion = servicioAutenticacion;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var resultado = _servicioAutenticacion.Autenticar(model.Username, model.Password);

            if (resultado.Exitoso && resultado.Usuario != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, resultado.Usuario.Nombre),
                    new Claim(ClaimTypes.Role, resultado.Usuario.Rol.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                if (Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                // Redirect to root which maps to Login GET (303 forces GET)
                Response.StatusCode = 303;
                Response.Headers.Location = "/";
                return new EmptyResult();
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
        }

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    public IActionResult AccessDenied() => View();
}