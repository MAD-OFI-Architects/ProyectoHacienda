using Hacienda.Domain.Enums;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Domain.Entities;

public class Usuario
{
    public Guid Id { get; }
    public string Nombre { get; }
    public Credencial Credencial { get; }
    public RolUsuario Rol { get; }

    public Usuario(Guid id, string nombre, Credencial credencial, RolUsuario rol)
    {
        Id = id;
        Nombre = nombre;
        Credencial = credencial;
        Rol = rol;
    }
}