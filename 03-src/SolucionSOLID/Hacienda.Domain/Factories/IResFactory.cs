using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;

namespace Hacienda.Domain.Factories;

public interface IResFactory
{
    Res Crear(TipoRes tipo, string nombre, uint peso, ushort edad);
}