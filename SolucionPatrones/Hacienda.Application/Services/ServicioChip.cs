using Hacienda.Application.Interfaces;
using Hacienda.Application.Results;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Application.Services;

public class ServicioChip : IServicioChip
{
    private readonly IRepositorioChip _repoChip;
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IInstaladorChip _instaladorChip;

    public ServicioChip(
        IRepositorioChip repoChip,
        IRepositorioPotrero repoPotrero,
        IInstaladorChip instaladorChip)
    {
        _repoChip = repoChip;
        _repoPotrero = repoPotrero;
        _instaladorChip = instaladorChip;
    }

    public ResultadoOperacion InstalarChip(Guid resId, string numeroSerie)
        => _instaladorChip.Instalar(resId, numeroSerie);

    public ResultadoOperacion CambiarEstadoChip(string numeroSerie, EstadoChip estado)
        => _instaladorChip.CambiarEstado(numeroSerie, estado);

    public Chip? ObtenerChipPorNumeroSerie(string numeroSerie)
        => _repoChip.ObtenerPorNumeroSerie(numeroSerie);

    public Chip? ObtenerChipPorResId(Guid resId)
    {
        var (_, res) = _repoPotrero.ObtenerTodos()
            .Select(p => (Potrero: p, Res: p.Reses.FirstOrDefault(r => r.Id == resId)))
            .FirstOrDefault(x => x.Res != null);

        return res?.Chip;
    }

    public List<Chip> ListarChips()
        => _repoChip.ObtenerTodos();
}