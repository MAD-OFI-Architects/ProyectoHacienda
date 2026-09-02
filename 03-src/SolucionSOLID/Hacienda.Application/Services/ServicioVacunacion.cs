using Hacienda.Application.Interfaces;
using Hacienda.Application.Results;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Events;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Interfaces;

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

    public ResultadoOperacion CrearVacunaBacteriana(string nombre, string lote,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo)
    {
        try
        {
            var vacunas = _repoVacuna.ObtenerTodas();
            ValidarLoteNoDuplicado(vacunas, lote);

            var vacuna = _fabricaVacuna.CrearBacteriana(nombre, lote, fechaVenc, fechaAplic, periodo);
            vacunas.Add(vacuna);
            _repoVacuna.GuardarTodas(vacunas);

            return ResultadoOperacion.Ok($"Vacuna bacteriana '{nombre}' (lote '{lote}') agregada al inventario.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return ResultadoOperacion.Fallo(ex.Message);
        }
    }

    public ResultadoOperacion CrearVacunaViva(string nombre, string lote,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion)
    {
        try
        {
            var vacunas = _repoVacuna.ObtenerTodas();
            ValidarLoteNoDuplicado(vacunas, lote);

            var vacuna = _fabricaVacuna.CrearViva(nombre, lote, fechaVenc, fechaAplic, atenuacion);
            vacunas.Add(vacuna);
            _repoVacuna.GuardarTodas(vacunas);

            return ResultadoOperacion.Ok($"Vacuna viva '{nombre}' (lote '{lote}') agregada al inventario.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return ResultadoOperacion.Fallo(ex.Message);
        }
    }

    public ResultadoOperacion CrearLoteVacunaBacteriana(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo, uint cantidad)
    {
        try
        {
            ValidarCantidadLote(cantidad);

            var vacunas = _repoVacuna.ObtenerTodas();
            var vacunasCreadas = 0;

            for (var i = 1; i <= cantidad; i++)
            {
                var loteNumerado = $"{loteBase}-{i:D3}";
                if (vacunas.Any(v => v.Lote.Equals(loteNumerado, StringComparison.OrdinalIgnoreCase)))
                    continue;

                vacunas.Add(_fabricaVacuna.CrearBacteriana(nombre, loteNumerado, fechaVenc, fechaAplic, periodo));
                vacunasCreadas++;
            }

            if (vacunasCreadas == 0)
                throw new InvalidOperationException("No se pudo crear ninguna vacuna. Todos los lotes ya existen");

            _repoVacuna.GuardarTodas(vacunas);

            return ResultadoOperacion.Ok($"Lote de vacunas bacterianas creado: {vacunasCreadas} de {cantidad}. " +
               $"Lotes: {loteBase}-001 a {loteBase}-{vacunasCreadas:D3}. Período: {periodo} semanas.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return ResultadoOperacion.Fallo(ex.Message);
        }
    }

    public ResultadoOperacion CrearLoteVacunaViva(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion, uint cantidad)
    {
        try
        {
            ValidarCantidadLote(cantidad);

            var vacunas = _repoVacuna.ObtenerTodas();
            var vacunasCreadas = 0;

            for (var i = 1; i <= cantidad; i++)
            {
                var loteNumerado = $"{loteBase}-{i:D3}";
                if (vacunas.Any(v => v.Lote.Equals(loteNumerado, StringComparison.OrdinalIgnoreCase)))
                    continue;

                vacunas.Add(_fabricaVacuna.CrearViva(nombre, loteNumerado, fechaVenc, fechaAplic, atenuacion));
                vacunasCreadas++;
            }

            if (vacunasCreadas == 0)
                throw new InvalidOperationException("No se pudo crear ninguna vacuna. Todos los lotes ya existen");

            _repoVacuna.GuardarTodas(vacunas);

            return ResultadoOperacion.Ok($"Lote de vacunas vivas creado: {vacunasCreadas} de {cantidad}. " +
               $"Lotes: {loteBase}-001 a {loteBase}-{vacunasCreadas:D3}. Atenuación: {(byte)atenuacion}.");
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return ResultadoOperacion.Fallo(ex.Message);
        }
    }

    public ResultadoOperacion AplicarVacuna(string loteVacuna, string potreroId, string nombreRes)
    {
        try
        {
            var vacunas = _repoVacuna.ObtenerTodas();
            var vacuna = vacunas.FirstOrDefault(v => v.Lote.Equals(loteVacuna, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Vacuna con lote '{loteVacuna}' no encontrada");

            var potreros = _repoPotrero.ObtenerTodos();
            var res = ObtenerResEnPotrero(potreros, potreroId, nombreRes);

            res.AplicarVacuna(vacuna);
            vacunas.Remove(vacuna);

            _repoVacuna.GuardarTodas(vacunas);
            _repoPotrero.GuardarTodos(potreros);
            _repoVacuna.GuardarAplicadas(potreros);

            var bacFinal = res.VacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Bacteriana);
            var vivFinal = res.VacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Viva);
            var mensajeEsquema = CrearMensajeEsquemaVacunacion(res, bacFinal, vivFinal);

            return ResultadoOperacion.Ok($"Vacuna '{vacuna.Nombre}' aplicada a '{nombreRes}' correctamente. Datos válidos. Guardado exitoso en BD.{mensajeEsquema}");
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return ResultadoOperacion.Fallo(ex.Message);
        }
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

    private static void ValidarLoteNoDuplicado(IEnumerable<Vacuna> vacunas, string lote)
    {
        if (vacunas.Any(v => v.Lote.Equals(lote, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Ya existe una vacuna con lote '{lote}'");
    }

    private static void ValidarCantidadLote(uint cantidad)
    {
        if (cantidad == 0 || cantidad > 100)
            throw new ArgumentException("La cantidad debe estar entre 1 y 100");
    }

    private Res ObtenerResEnPotrero(List<Potrero> potreros, string potreroId, string nombreRes)
    {
        var potrero = potreros.FirstOrDefault(p => p.Identificacion.Valor.Equals(potreroId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Potrero '{potreroId}' no encontrado");

        return potrero.BuscarRes(nombreRes)
            ?? throw new InvalidOperationException($"Res '{nombreRes}' no encontrada");
    }

    private string CrearMensajeEsquemaVacunacion(Res res, int bacFinal, int vivFinal)
    {
        if (res.EsquemaVacunacionCompleto())
        {
            _eventPublisher.Publicar(new VacunacionCompletadaEvent(res.Nombre, _reloj.GetUtcNow().DateTime));
            return $" Esquema de vacunación COMPLETADO para '{res.Nombre}'.";
        }

        return $" La res '{res.Nombre}' aún no ha completado su esquema de vacunación. Bacterianas: {bacFinal}, Vivas: {vivFinal}.";
    }
}