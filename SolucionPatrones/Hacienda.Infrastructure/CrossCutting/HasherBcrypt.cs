using Hacienda.Domain.Interfaces;
using BCrypt.Net;

namespace Hacienda.Infrastructure.CrossCutting;

public class HasherBcrypt : IHasher
{
    public string Hashear(string passwordPlano)
        => BCrypt.Net.BCrypt.HashPassword(passwordPlano);

    public bool Verificar(string passwordPlano, string passwordHash)
        => BCrypt.Net.BCrypt.Verify(passwordPlano, passwordHash);
}