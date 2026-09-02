using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Application.Services;

public class ResLocator : IResLocator
{
    private readonly IRepositorioPotrero _repoPotrero;

    public ResLocator(IRepositorioPotrero repoPotrero)
    {
        _repoPotrero = repoPotrero;
    }

    public (Potrero? Potrero, Res? Res) BuscarPorId(Guid resId)
    {
        var potreros = _repoPotrero.ObtenerTodos();
        var potrero = potreros.FirstOrDefault(p => p.Reses.Any(r => r.Id == resId));
        var res = potrero?.Reses.FirstOrDefault(r => r.Id == resId);
        return (potrero, res);
    }
}
