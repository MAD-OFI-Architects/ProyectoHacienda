using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Application.Services;

public class ServicioChip : IServicioChip
{
    private readonly IRepositorioChip _repoChip;
    private readonly IRepositorioRes _repoRes;
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IRepositorioVacuna _repoVacuna;
    private readonly IGuidProvider _guidProvider;
    private readonly TimeProvider _reloj;

    public ServicioChip(
        IRepositorioChip repoChip,
        IRepositorioRes repoRes,
        IRepositorioPotrero repoPotrero,
        IRepositorioVacuna repoVacuna,
        IGuidProvider guidProvider,
        TimeProvider reloj)
    {
        _repoChip = repoChip;
        _repoRes = repoRes;
        _repoPotrero = repoPotrero;
        _repoVacuna = repoVacuna;
        _guidProvider = guidProvider;
        _reloj = reloj;
    }

public string InstalarChip(Guid resId, string numeroSerieStr)
{
    var potreros = _repoPotrero.ObtenerTodos();
    var potrero = potreros.FirstOrDefault(p => p.Reses.Any(r => r.Id == resId));
    var res = potrero?.Reses.FirstOrDefault(r => r.Id == resId);

    if (res == null)
        return $"Res con ID {resId} no encontrada";

    if (res.Chip != null)
        return $"La res ya tiene un chip instalado ({res.Chip.NumeroSerie})";

    var chipExistente = _repoChip.ObtenerPorNumeroSerie(numeroSerieStr);
    if (chipExistente != null)
        return $"Ya existe un chip con el número de serie {numeroSerieStr}";

    var numeroSerie = new NumeroSerieChip(numeroSerieStr);
    var chip = Chip.Crear(_guidProvider.Nuevo(), numeroSerie, _reloj.GetUtcNow().DateTime);
    res.Chip = chip;

    _repoChip.Guardar(chip);
    _repoPotrero.GuardarTodos(potreros);
    _repoVacuna.GuardarAplicadas(potreros);

    return $"Chip {numeroSerieStr} instalado correctamente en la res {res.Nombre}";
}

    public string CambiarEstadoChip(string numeroSerie, EstadoChip estado)
    {
        var chip = _repoChip.ObtenerPorNumeroSerie(numeroSerie);
        if (chip == null)
            return $"Chip con número de serie {numeroSerie} no encontrado";

        try
        {
            chip.CambiarEstado(estado);
            _repoChip.Guardar(chip);
            return $"Estado del chip {numeroSerie} cambiado a {estado}";
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
        catch (ArgumentException ex)
        {
            return ex.Message;
        }
    }

    public IChip? ObtenerChipPorNumeroSerie(string numeroSerie)
        => _repoChip.ObtenerPorNumeroSerie(numeroSerie);

public IChip? ObtenerChipPorResId(Guid resId)
{
    var potrero = _repoPotrero.ObtenerTodos().FirstOrDefault(p => p.Reses.Any(r => r.Id == resId));
    var res = potrero?.Reses.FirstOrDefault(r => r.Id == resId);
    return res?.Chip;
}

    public List<IChip> ListarChips()
        => _repoChip.ObtenerTodos();
}