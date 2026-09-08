using Hacienda.Application.Interfaces;
using Hacienda.Application.Results;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.Reglas;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Application.Services;

public class InstaladorChip : IInstaladorChip
{
    private readonly IRepositorioChip _repoChip;
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IRepositorioVacuna _repoVacuna;
    private readonly IGuidProvider _guidProvider;
    private readonly TimeProvider _reloj;

    public InstaladorChip(
        IRepositorioChip repoChip,
        IRepositorioPotrero repoPotrero,
        IRepositorioVacuna repoVacuna,
        IGuidProvider guidProvider,
        TimeProvider reloj)
    {
        _repoChip = repoChip;
        _repoPotrero = repoPotrero;
        _repoVacuna = repoVacuna;
        _guidProvider = guidProvider;
        _reloj = reloj;
    }

    public ResultadoOperacion Instalar(Guid resId, string numeroSerieStr)
    {
        // Grafo ÚNICO: se lee una sola vez, se modifica en memoria y se persiste esa misma instancia.
        // (Releer con ObtenerTodos() después de mutar descarta el cambio: el chip_id nunca se guardaba.)
        var potreros = _repoPotrero.ObtenerTodos();
        var potrero = potreros.FirstOrDefault(p => p.Reses.Any(r => r.Id == resId));
        var res = potrero?.Reses.FirstOrDefault(r => r.Id == resId);

        if (res == null)
            return ResultadoOperacion.Fallo($"Res con ID {resId} no encontrada");

        if (res.Chip != null)
            return ResultadoOperacion.Fallo($"La res ya tiene un chip instalado ({res.Chip.NumeroSerie})");

        if (_repoChip.ObtenerPorNumeroSerie(numeroSerieStr) != null)
            return ResultadoOperacion.Fallo($"Ya existe un chip con el número de serie {numeroSerieStr}");

        Chip chip;
        try
        {
            chip = CrearChip(numeroSerieStr);
        }
        catch (ArgumentException ex)
        {
            return ResultadoOperacion.Fallo(ex.Message);
        }

        res.InstalarChip(chip);
        _repoChip.Guardar(chip);
        _repoPotrero.GuardarTodos(potreros);
        _repoVacuna.GuardarAplicadas(potreros);

        return ResultadoOperacion.Ok($"Chip {numeroSerieStr} instalado correctamente en la res {res.Nombre}");
    }

    public ResultadoOperacion CambiarEstado(string numeroSerie, EstadoChip estado)
    {
        var chip = _repoChip.ObtenerPorNumeroSerie(numeroSerie);
        if (chip == null)
            return ResultadoOperacion.Fallo($"Chip con número de serie {numeroSerie} no encontrado");

        try
        {
            chip.CambiarEstado(estado);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return ResultadoOperacion.Fallo(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ResultadoOperacion.Fallo(ex.Message);
        }

        _repoChip.Guardar(chip);
        return ResultadoOperacion.Ok($"Estado del chip {numeroSerie} cambiado a {estado}");
    }

    private Chip CrearChip(string numeroSerieStr)
    {
        var numeroSerie = new NumeroSerieChip(numeroSerieStr);
        var fechaInstalacion = _reloj.GetUtcNow().DateTime;
        return new Chip(_guidProvider.Nuevo(), numeroSerie, fechaInstalacion);
    }
}
