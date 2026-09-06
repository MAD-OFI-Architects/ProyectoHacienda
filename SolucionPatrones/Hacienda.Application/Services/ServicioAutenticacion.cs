using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.Results;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Application.Services;

public class ServicioAutenticacion : IServicioAutenticacion
{
    private readonly IRepositorioUsuario _repoUsuario;
    private readonly IHasher _hasher;
    private readonly IGuidProvider _guidProvider;

    public ServicioAutenticacion(IRepositorioUsuario repoUsuario, IHasher hasher, IGuidProvider guidProvider)
    {
        _repoUsuario = repoUsuario;
        _hasher = hasher;
        _guidProvider = guidProvider;
    }

    public ResultadoAutenticacion Autenticar(string username, string password)
    {
        var usuarios = _repoUsuario.ObtenerTodos();
        var usuario = usuarios.FirstOrDefault(u =>
            u.Nombre.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (usuario == null)
            return ResultadoAutenticacion.Fallido($"Usuario '{username}' no encontrado");

        if (!usuario.Credencial.Verificar(password, _hasher))
            return ResultadoAutenticacion.Fallido("Credenciales inválidas");

        return ResultadoAutenticacion.Ok(usuario);
    }

    public List<Usuario> ObtenerTodosLosUsuarios()
    {
        return _repoUsuario.ObtenerTodos();
    }

    public (bool Exitoso, string Mensaje) CrearUsuario(string nombre, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre del usuario no puede estar vacío");

        if (string.IsNullOrWhiteSpace(contrasena))
            return (false, "La contraseña no puede estar vacía");

        var usuarios = _repoUsuario.ObtenerTodos();

        if (usuarios.Any(u => u.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            return (false, $"Ya existe un usuario con el nombre '{nombre}'");

        var credencial = Credencial.DesdePasswordPlano(contrasena, _hasher);
        var nuevoUsuario = new Usuario(_guidProvider.Nuevo(), nombre, credencial, RolUsuario.Visitante);

        usuarios.Add(nuevoUsuario);
        _repoUsuario.GuardarTodos(usuarios);

        return (true, $"Usuario '{nombre}' creado exitosamente");
    }
}