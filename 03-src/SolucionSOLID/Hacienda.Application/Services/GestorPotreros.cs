using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Services;

public class GestorPotreros : IGestorPotreros
{
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IValidarPotrero _validador;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IPotreroFactory _fabricaPotrero;

    public GestorPotreros(
        IRepositorioPotrero repoPotrero,
        IValidarPotrero validador,
        IDomainEventPublisher eventPublisher,
        IPotreroFactory fabricaPotrero)
    {
        _repoPotrero = repoPotrero;
        _validador = validador;
        _eventPublisher = eventPublisher;
        _fabricaPotrero = fabricaPotrero;
    }

    public string CrearPotrero(string identificacion, TipoPotrero tipo)
    {
        if (_repoPotrero.ObtenerPorIdentificacion(identificacion) != null)
            throw new InvalidOperationException($"Ya existe un potrero '{identificacion}'");

        var potrero = _fabricaPotrero.Crear(identificacion, tipo);

        var validacion = _validador.Validar(potrero);
        if (!validacion.EsValido)
            return string.Join("; ", validacion.Errores);

        var potreros = _repoPotrero.ObtenerTodos();
        potreros.Add(potrero);
        _repoPotrero.GuardarTodos(potreros);

        return $"El potrero '{identificacion}' se añadió con éxito.";
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