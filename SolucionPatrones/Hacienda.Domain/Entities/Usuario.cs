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
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador del usuario no puede ser vacío", nameof(id));

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del usuario no puede estar vacío", nameof(nombre));

        if (credencial == null)
            throw new ArgumentNullException(nameof(credencial));

        Id = id;
        Nombre = nombre.Trim();
        Credencial = credencial;
        Rol = rol;
    }
}