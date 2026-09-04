using Hacienda.Application.Interfaces;
using Hacienda.Application.Results;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Services;

public class GestorPotreros : IGestorPotreros
{
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IPotreroFactory _fabricaPotrero;

    public GestorPotreros(
        IRepositorioPotrero repoPotrero,
        IDomainEventPublisher eventPublisher,
        IPotreroFactory fabricaPotrero)
    {
        _repoPotrero = repoPotrero;
        _eventPublisher = eventPublisher;
        _fabricaPotrero = fabricaPotrero;
    }

    public ResultadoOperacion CrearPotrero(string identificacion, TipoPotrero tipo)
    {
        if (_repoPotrero.ObtenerPorIdentificacion(identificacion) != null)
            return ResultadoOperacion.Fallo($"Ya existe un potrero '{identificacion}'");

        var potrero = _fabricaPotrero.Crear(identificacion, tipo);

        var potreros = _repoPotrero.ObtenerTodos();
        potreros.Add(potrero);
        _repoPotrero.GuardarTodos(potreros);

        return ResultadoOperacion.Ok($"El potrero '{identificacion}' se añadió con éxito.");
    }

    public Potrero? BuscarPotrero(string identificacion)
        => _repoPotrero.ObtenerPorIdentificacion(identificacion);

    public List<Potrero> ListarPotreros()
        => _repoPotrero.ObtenerTodos().OrderBy(p => p.Identificacion.Valor).ToList();

    public Dictionary<string, object> ObtenerEstadisticas()
    {
        var potreros = _repoPotrero.ObtenerTodos();
        return new Dictionary<string, object>
        {
            ["TotalPotreros"] = potreros.Count,
            ["TotalReses"] = potreros.Sum(p => p.CantidadReses),
            ["PotrerosVacios"] = potreros.Count(p => p.CantidadReses == 0),
            ["PotrerosConReses"] = potreros.Count(p => p.CantidadReses > 0)
        };
    }
}