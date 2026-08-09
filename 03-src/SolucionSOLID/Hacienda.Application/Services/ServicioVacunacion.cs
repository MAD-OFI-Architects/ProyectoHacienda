using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.Results;
using Hacienda.Domain.Events;

namespace Hacienda.Application.Services;

public class ServicioVacunacion : IServicioVacunacion
{
    private readonly IRepositorioVacuna _repoVacuna;
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IVacunaFactory _fabricaVacuna;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly TimeProvider _reloj;

    public ServicioVacunacion(
        IRepositorioVacuna repoVacuna,
        IRepositorioPotrero repoPotrero,
        IVacunaFactory fabricaVacuna,
        IDomainEventPublisher eventPublisher,
        TimeProvider reloj)
    {
        _repoVacuna = repoVacuna;
        _repoPotrero = repoPotrero;
        _fabricaVacuna = fabricaVacuna;
        _eventPublisher = eventPublisher;
        _reloj = reloj;
    }

    public string CrearVacunaBacteriana(string nombre, string lote,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo)
    {
        var vacunas = _repoVacuna.ObtenerTodas();
        if (vacunas.Any(v => v.Lote.Equals(lote, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Ya existe una vacuna con lote '{lote}'");

        Vacuna vacuna = _fabricaVacuna.CrearBacteriana(nombre, lote, fechaVenc, fechaAplic, periodo);
        vacunas.Add(vacuna);
        _repoVacuna.GuardarTodas(vacunas);

        return $"Vacuna bacteriana '{nombre}' (lote '{lote}') agregada al inventario.";
    }

    public string CrearVacunaViva(string nombre, string lote,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion)
    {
        var vacunas = _repoVacuna.ObtenerTodas();
        if (vacunas.Any(v => v.Lote.Equals(lote, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Ya existe una vacuna con lote '{lote}'");

        Vacuna vacuna = _fabricaVacuna.CrearViva(nombre, lote, fechaVenc, fechaAplic, atenuacion);
        vacunas.Add(vacuna);
        _repoVacuna.GuardarTodas(vacunas);

        return $"Vacuna viva '{nombre}' (lote '{lote}') agregada al inventario.";
    }

    public string CrearLoteVacunaBacteriana(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo, uint cantidad)
    {
        if (cantidad == 0 || cantidad > 100)
            throw new ArgumentException("La cantidad debe estar entre 1 y 100");

        var vacunas = _repoVacuna.ObtenerTodas();
        int vacunasCreadas = 0;

        for (int i = 1; i <= cantidad; i++)
        {
            string loteNumerado = $"{loteBase}-{i:D3}";
            if (vacunas.Any(v => v.Lote.Equals(loteNumerado, StringComparison.OrdinalIgnoreCase)))
                continue;

            Vacuna vacuna = _fabricaVacuna.CrearBacteriana(nombre, loteNumerado, fechaVenc, fechaAplic, periodo);
            vacunas.Add(vacuna);
            vacunasCreadas++;
        }

        if (vacunasCreadas == 0)
            throw new InvalidOperationException("No se pudo crear ninguna vacuna. Todos los lotes ya existen");

        _repoVacuna.GuardarTodas(vacunas);

        return $"Lote de vacunas bacterianas creado: {vacunasCreadas} de {cantidad}. " +
               $"Lotes: {loteBase}-001 a {loteBase}-{vacunasCreadas:D3}. Período: {periodo} semanas.";
    }

    public string CrearLoteVacunaViva(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion, uint cantidad)
    {
        if (cantidad == 0 || cantidad > 100)
            throw new ArgumentException("La cantidad debe estar entre 1 y 100");

        var vacunas = _repoVacuna.ObtenerTodas();
        int vacunasCreadas = 0;

        for (int i = 1; i <= cantidad; i++)
        {
            string loteNumerado = $"{loteBase}-{i:D3}";
            if (vacunas.Any(v => v.Lote.Equals(loteNumerado, StringComparison.OrdinalIgnoreCase)))
                continue;

            Vacuna vacuna = _fabricaVacuna.CrearViva(nombre, loteNumerado, fechaVenc, fechaAplic, atenuacion);
            vacunas.Add(vacuna);
            vacunasCreadas++;
        }

        if (vacunasCreadas == 0)
            throw new InvalidOperationException("No se pudo crear ninguna vacuna. Todos los lotes ya existen");

        _repoVacuna.GuardarTodas(vacunas);

        return $"Lote de vacunas vivas creado: {vacunasCreadas} de {cantidad}. " +
               $"Lotes: {loteBase}-001 a {loteBase}-{vacunasCreadas:D3}. Atenuación: {(byte)atenuacion}.";
    }

    public string AplicarVacuna(string loteVacuna, string potreroId, string nombreRes)
    {
        var vacunas = _repoVacuna.ObtenerTodas();
        var vacuna = vacunas.FirstOrDefault(v => v.Lote.Equals(loteVacuna, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Vacuna con lote '{loteVacuna}' no encontrada");

        var potreros = _repoPotrero.ObtenerTodos();
        var potrero = potreros.FirstOrDefault(p => p.Identificacion.Valor.Equals(potreroId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Potrero '{potreroId}' no encontrado");
        var res = potrero.BuscarRes(nombreRes)
            ?? throw new InvalidOperationException($"Res '{nombreRes}' no encontrada");

        if (res.VacunasAplicadas.Any(v => v.Lote.Equals(loteVacuna, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"La vacuna lote '{loteVacuna}' ya fue aplicada a '{nombreRes}'");

        int bacActual = res.VacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Bacteriana);
        int vivActual = res.VacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Viva);

        if (vacuna.Categoria == VacunaCategoria.Bacteriana && bacActual >= res.MaxVacunasBacterianas)
            throw new InvalidOperationException(
                $"No se pueden aplicar más bacterianas a '{nombreRes}' (máximo {res.MaxVacunasBacterianas})");

        if (vacuna.Categoria == VacunaCategoria.Viva && vivActual >= res.MaxVacunasVivas)
            throw new InvalidOperationException(
                $"No se pueden aplicar más vivas a '{nombreRes}' (máximo {res.MaxVacunasVivas})");

        res.VacunasAplicadas.Add(vacuna);
        vacunas.Remove(vacuna);

        _repoVacuna.GuardarTodas(vacunas);
        _repoPotrero.GuardarTodos(potreros);
        _repoVacuna.GuardarAplicadas(potreros);

        int bacFinal = res.VacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Bacteriana);
        int vivFinal = res.VacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Viva);

        string mensajeEsquema = "";
        if (res.EsquemaVacunacionCompleto())
        {
            _eventPublisher.Publicar(new VacunacionCompletadaEvent(res.Nombre, _reloj.GetUtcNow().DateTime));
            mensajeEsquema = $" Esquema de vacunación COMPLETADO para '{res.Nombre}'.";
        }
        else
        {
            mensajeEsquema = $" La res '{res.Nombre}' aún no ha completado su esquema de vacunación. Bacterianas: {bacFinal}, Vivas: {vivFinal}.";
        }

        return $"Vacuna '{vacuna.Nombre}' aplicada a '{nombreRes}' correctamente. Datos válidos. Guardado exitoso en BD.{mensajeEsquema}";
    }

    public List<Vacuna> ListarVacunasDisponibles()
        => _repoVacuna.ObtenerTodas().OrderBy(v => v.Nombre).ToList();

    public Dictionary<string, object> ObtenerEstadisticas()
    {
        var vacunas = _repoVacuna.ObtenerTodas();
        return new Dictionary<string, object>
        {
            ["TotalVacunas"] = vacunas.Count,
            ["Bacterianas"] = vacunas.Count(v => v.Categoria == VacunaCategoria.Bacteriana),
            ["Vivas"] = vacunas.Count(v => v.Categoria == VacunaCategoria.Viva),
            ["Vencidas"] = vacunas.Count(v => v.CalcularEstado(_reloj) == EstadoVacuna.Vencida),
            ["PorVencer"] = vacunas.Count(v => v.CalcularEstado(_reloj) == EstadoVacuna.PorVencer),
            ["Vigentes"] = vacunas.Count(v => v.CalcularEstado(_reloj) == EstadoVacuna.Vigente)
        };
    }
}