namespace Hacienda.Domain.Interfaces;

public interface IHasher
{
    string Hashear(string passwordPlano);
    bool Verificar(string passwordPlano, string passwordHash);
}