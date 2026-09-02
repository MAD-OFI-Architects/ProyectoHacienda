using Hacienda.Application.Interfaces;
using Hacienda.Application.Results;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Events;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Application.Services;

public class GestorReses : IGestorReses
{
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IRepositorioVacuna _repoVacuna;
    private readonly IResFactory _fabricaRes;
    private readonly IValidarRes _validador;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly TimeProvider _reloj;

    public GestorReses(
        IRepositorioPotrero repoPotrero,
        IRepositorioVacuna repoVacuna,
        IResFactory fabricaRes,
        IValidarRes validador,
        IDomainEventPublisher eventPublisher,
        TimeProvider reloj)
    {
        _repoPotrero = repoPotrero;
        _repoVacuna = repoVacuna;
        _fabricaRes = fabricaRes;
        _validador = validador;
        _eventPublisher = eventPublisher;
        _reloj = reloj;
    }

    public ResultadoOperacion AgregarRes(string potreroId, string nombre, ushort edad, uint peso)
    {
        var potreros = _repoPotrero.ObtenerTodos();
        var potrero = potreros.FirstOrDefault(p => p.Identificacion.Valor.Equals(potreroId, StringComparison.OrdinalIgnoreCase));
        if (potrero is null)
            return ResultadoOperacion.Fallo($"Potrero '{potreroId}' no encontrado");

        TipoRes tipo = MapearTipoRes(potrero.Tipo);
        Res res = _fabricaRes.Crear(tipo, nombre, peso, edad);

        var validacion = _validador.Validar(res);
        if (!validacion.EsValido)
            return ResultadoOperacion.Fallo(validacion.Errores);

        potrero.AgregarRes(res);

        string mensajeEventos = "";
        if (res.Peso < res.PesoMinimo)
        {
            _eventPublisher.Publicar(new PesoMinimoEvent(res.Nombre, res.Peso, _reloj.GetUtcNow().DateTime));
            mensajeEventos += $"\n[Evento] La res '{res.Nombre}' está en desnutrición ({res.Peso} kg).";
        }
        if (res.Peso >= res.PesoRecomendadoVenta)
        {
            _eventPublisher.Publicar(new PesoVentaEvent(res.Nombre, res.Peso, _reloj.GetUtcNow().DateTime));
            mensajeEventos += $"\n[Evento] La res '{res.Nombre}' está apta para venta ({res.Peso} kg).";
        }
        if (potrero.EstaALaMitad)
        {
            _eventPublisher.Publicar(new PotreroMitadEvent(potreroId, _reloj.GetUtcNow().DateTime));
            mensajeEventos += $"\n[Evento] El potrero '{potreroId}' alcanzó la mitad de su capacidad.";
        }
        if (potrero.EstaLleno)
        {
            _eventPublisher.Publicar(new PotreroLlenoEvent(potreroId, _reloj.GetUtcNow().DateTime));
            mensajeEventos += $"\n[Evento] El potrero '{potreroId}' está lleno.";
        }

        _repoPotrero.GuardarTodos(potreros);

        return ResultadoOperacion.Ok($"La res '{nombre}' fue añadida al potrero '{potreroId}'.{mensajeEventos}");
    }

    public ResultadoOperacion AlimentarRes(string potreroId, string nombreRes)
        => AlimentarRes(potreroId, nombreRes, 1);

    public ResultadoOperacion AlimentarRes(string potreroId, string nombreRes, uint cantidad)
    {
        var potreros = _repoPotrero.ObtenerTodos();
        var potrero = potreros.FirstOrDefault(p => p.Identificacion.Valor.Equals(potreroId, StringComparison.OrdinalIgnoreCase));
        if (potrero is null)
            return ResultadoOperacion.Fallo($"Potrero '{potreroId}' no encontrado");

        var res = potrero.BuscarRes(nombreRes);
        if (res is null)
            return ResultadoOperacion.Fallo($"Res '{nombreRes}' no encontrada");

        res.Alimentar(cantidad);

        string mensajeEventos = "";
        if (res.Peso < res.PesoMinimo)
        {
            _eventPublisher.Publicar(new PesoMinimoEvent(res.Nombre, res.Peso, _reloj.GetUtcNow().DateTime));
            mensajeEventos += $"\n[Evento] La res '{res.Nombre}' sigue en desnutrición ({res.Peso} kg).";
        }
        if (res.Peso >= res.PesoRecomendadoVenta)
        {
            _eventPublisher.Publicar(new PesoVentaEvent(res.Nombre, res.Peso, _reloj.GetUtcNow().DateTime));
            mensajeEventos += $"\n[Evento] La res '{res.Nombre}' está apta para venta ({res.Peso} kg).";
        }

        _repoPotrero.GuardarTodos(potreros);

        return ResultadoOperacion.Ok($"La res '{res.Nombre}' fue alimentada, ahora pesa {res.Peso} kg.{mensajeEventos}");
    }

    public List<(Potrero Potrero, Res Res)> ListarReses()
    {
        var potreros = _repoPotrero.ObtenerTodos();
        _repoVacuna.CargarVacunasAplicadasEnPotreros(potreros);

        var resultado = new List<(Potrero, Res)>();
        foreach (var potrero in potreros)
            foreach (var res in potrero.Reses)
                resultado.Add((potrero, res));
        return resultado;
    }

    public Dictionary<string, object> ObtenerEstadisticas()
    {
        var todas = ListarReses();
        return new Dictionary<string, object>
        {
            ["TotalReses"] = todas.Count,
            ["Terneros"] = todas.Count(r => r.Res.Tipo == TipoRes.Ternero),
            ["Cebones"] = todas.Count(r => r.Res.Tipo == TipoRes.Cebon),
            ["Novillos"] = todas.Count(r => r.Res.Tipo == TipoRes.Novillo),
            ["PesoPromedio"] = todas.Any() ? todas.Average(r => r.Res.Peso) : 0
        };
    }

    private static TipoRes MapearTipoRes(TipoPotrero tipoPotrero) => CatalogoRes.MapearDesdePotrero(tipoPotrero);
}