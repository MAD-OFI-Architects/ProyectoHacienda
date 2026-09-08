using Hacienda.Domain.Entities;

namespace Hacienda.Application.Interfaces;

public interface IResLocator
{
    (Potrero? Potrero, Res? Res) BuscarPorId(Guid resId);
}
