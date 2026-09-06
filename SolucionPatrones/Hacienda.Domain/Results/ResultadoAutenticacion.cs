using Hacienda.Domain.Entities;

namespace Hacienda.Domain.Results;

public sealed record ResultadoAutenticacion
{
    public bool Exitoso { get; }
    public Usuario? Usuario { get; }
    public string Mensaje { get; }

    private ResultadoAutenticacion(bool exitoso, Usuario? usuario, string mensaje)
    {
        Exitoso = exitoso;
        Usuario = usuario;
        Mensaje = mensaje;
    }

    public static ResultadoAutenticacion Ok(Usuario usuario)
        => new(true, usuario, $"Autenticación exitosa para '{usuario.Nombre}'");
    public static ResultadoAutenticacion Fallido(string motivo)
        => new(false, null, motivo);
}