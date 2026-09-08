using Hacienda.Domain.Entities;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Interfaces;

public interface IServicioAutenticacion
{
    ResultadoAutenticacion Autenticar(string username, string password);
    List<Usuario> ObtenerTodosLosUsuarios();
    (bool Exitoso, string Mensaje) CrearUsuario(string nombre, string contrasena);
}