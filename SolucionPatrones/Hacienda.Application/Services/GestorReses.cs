using Hacienda.Application.Interfaces;
using Hacienda.Application.Results;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Events;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.Reglas;

namespace Hacienda.Application.Services;

public class GestorReses : IGestorReses
{
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IRegistroDeReses _registroReses;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly TimeProvider _reloj;

    public GestorReses(
        IRepositorioPotrero repoPotrero,
        IRegistroDeReses registroReses,
        IDomainEventPublisher eventPublisher,
        TimeProvider reloj)
    {
        _repoPotrero = repoPotrero;
        _registroReses = registroReses;
        _eventPublisher = eventPublisher;
        _reloj = reloj;
    }

    public ResultadoOperacion AgregarRes(string potreroId, string nombre, ushort edad, uint peso)
    {
        var potreros = _repoPotrero.ObtenerTodos();
        var potrero = potreros.FirstOrDefault(p => p.Identificacion.Valor.Equals(potreroId, StringComparison.OrdinalIgnoreCase));
        if (potrero is null)
            return ResultadoOperacion.Fallo($"Potrero '{potreroId}' no encontrado");

        TipoRes tipo = _registroReses.MapearDesdePotrero(potrero.Tipo);

        Res res;
        try
        {
            // Reglas del dominio (edad del subtipo en el ctor, y capacidad/duplicados en Potrero)
            // Lanzan excepciones: acá se traducen a fallo de negocio esperado, sin duplicar la regla.
            res = _registroReses.Crear(tipo, nombre, peso, edad);
            potrero.AgregarRes(res);
        }
        catch (InvalidOperationException ex)
        {
            return ResultadoOperacion.Fallo(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return ResultadoOperacion.Fallo(ex.Message);
        }

        var errores = res.ValidarIntegridad();
        if (errores.Count > 0)
            return ResultadoOperacion.Fallo(errores);

        string mensajeEventos = EvaluarYPublicarPeso(res);
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

        string mensajeEventos = EvaluarYPublicarPeso(res);

        _repoPotrero.GuardarTodos(potreros);

        return ResultadoOperacion.Ok($"La res '{res.Nombre}' fue alimentada, ahora pesa {res.Peso} kg.{mensajeEventos}");
    }

    /// <summary>
    /// P-14: la reacción de peso vive UNA vez (antes duplicada entre AgregarRes y AlimentarRes).
    /// El flujo publica; los handlers del despachador reaccionan (consola con la misma línea de siempre).
    /// </summary>
    private string EvaluarYPublicarPeso(Res res)
    {
        string mensaje = "";
        if (res.Peso < res.PesoMinimo)
        {
            _eventPublisher.Publicar(new PesoMinimoEvent(res.Nombre, res.Peso, _reloj.GetUtcNow().DateTime));
            mensaje += $"\n[Evento] La res '{res.Nombre}' está en desnutrición ({res.Peso} kg).";
        }
        if (res.Peso >= res.PesoRecomendadoVenta)
        {
            _eventPublisher.Publicar(new PesoVentaEvent(res.Nombre, res.Peso, _reloj.GetUtcNow().DateTime));
            mensaje += $"\n[Evento] La res '{res.Nombre}' está apta para venta ({res.Peso} kg).";
        }
        return mensaje;
    }

    public List<(Potrero Potrero, Res Res)> ListarReses()
    {
        var potreros = _repoPotrero.ObtenerTodos();

        var resultado = new List<(Potrero, Res)>();
        foreach (var potrero in potreros)
            foreach (var res in potrero.Reses)
                resultado.Add((potrero, res));

        return resultado;
    }

    public Dictionary<string, object> ObtenerEstadisticas()
    {
        var todas = ListarReses();
        var stats = new Dictionary<string, object>
        {
            ["TotalReses"] = todas.Count,
            ["PesoPromedio"] = todas.Any() ? todas.Average(r => r.Res.Peso) : 0
        };

        // P-01: contadores polimórficos — el plural es dato del catálogo, sin cases por tipo.
        foreach (var tipo in Enum.GetValues<TipoRes>())
            stats[ParametrosRes.PluralPorTipo[tipo]] = todas.Count(r => r.Res.Tipo == tipo);

        return stats;
    }
}
