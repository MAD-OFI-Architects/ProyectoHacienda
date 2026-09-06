using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.ValueObjects;

public sealed record Credencial
{
    public string PasswordHash { get; }

    public Credencial(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash no puede ser vacío", nameof(passwordHash));
        PasswordHash = passwordHash;
    }

    public bool Verificar(string passwordPlano, IHasher hasher)
        => hasher.Verificar(passwordPlano, PasswordHash);

    public static Credencial DesdePasswordPlano(string password, IHasher hasher)
        => new Credencial(hasher.Hashear(password));
}