using Hacienda.Application.Interfaces;
using Hacienda.Application.Results;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Factories.Reses;
using Hacienda.Domain.Factories.Vacunas;
using Hacienda.Domain.Factories.Productos;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.Events;

namespace Hacienda.Application.Services;

public class ServicioVacunacion : IServicioVacunacion
{
    private readonly IRepositorioVacuna _repoVacuna;
    private readonly IRepositorioPotrero _repoPotrero;
    private readonly IRegistroDeVacunas _registroVacunas;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly TimeProvider _reloj;

    public ServicioVacunacion(
        IRepositorioVacuna repoVacuna,
        IRepositorioPotrero repoPotrero,
        IRegistroDeVacunas registroVacunas,
        IDomainEventPublisher eventPublisher,
        TimeProvider reloj)
    {
        _repoVacuna = repoVacuna;
        _repoPotrero = repoPotrero;
        _registroVacunas = registroVacunas;
        _eventPublisher = eventPublisher;
        _reloj = reloj;
    }

    /// <summary>Una sola puerta de creación (P-02): la categoría del DatosVacuna decide la fábrica.</summary>
    public ResultadoOperacion CrearVacuna(DatosVacuna datos)
    {
        var vacunas = _repoVacuna.ObtenerTodas();
        if (vacunas.Any(v => v.Lote.Equals(datos.Lote, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Ya existe una vacuna con lote '{datos.Lote}'");

        var errores = _registroVacunas.Validar(datos);
        if (errores.Count > 0)
            return ResultadoOperacion.Fallo(errores);

        var vacuna = _registroVacunas.Crear(datos);
        vacunas.Add(vacuna);
        _repoVacuna.GuardarTodas(vacunas);

        var categoria = datos.Categoria == VacunaCategoria.Bacteriana ? "bacteriana" : "viva";
        return ResultadoOperacion.Ok($"Vacuna {categoria} '{datos.Nombre}' (lote '{datos.Lote}') agregada al inventario.");
    }

    public ResultadoOperacion CrearLoteVacunaBacteriana(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, uint periodo, uint cantidad)
    {
        var datos = new DatosVacuna(VacunaCategoria.Bacteriana, nombre, loteBase, fechaVenc, fechaAplic, periodo);
        var (creadas, ultimoLote) = CrearLote(datos, cantidad);
        return ResultadoOperacion.Ok(
            $"Lote de vacunas bacterianas creado: {creadas} de {cantidad}. " +
            $"Lotes: {loteBase}-001 a {ultimoLote}. Período: {periodo} semanas.");
    }

    public ResultadoOperacion CrearLoteVacunaViva(string nombre, string loteBase,
        DateTime fechaVenc, DateTime fechaAplic, Viva.GradoAtenuacion atenuacion, uint cantidad)
    {
        var datos = new DatosVacuna(VacunaCategoria.Viva, nombre, loteBase, fechaVenc, fechaAplic, null, atenuacion);
        var (creadas, ultimoLote) = CrearLote(datos, cantidad);
        return ResultadoOperacion.Ok(
            $"Lote de vacunas vivas creado: {creadas} de {cantidad}. " +
            $"Lotes: {loteBase}-001 a {ultimoLote}. Atenuación: {(byte)atenuacion}.");
    }

    /// <summary>P-10: el esqueleto del lote vive UNA vez en FabricaDeVacuna.CrearLote (Template Method).</summary>
    private (int Creadas, string UltimoLote) CrearLote(DatosVacuna datos, uint cantidad)
    {
        var vacunas = _repoVacuna.ObtenerTodas();
        var creadas = _registroVacunas.FabricaPara(datos)
            .CrearLote(datos, cantidad, l => vacunas.Any(v => v.Lote.Equals(l, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (creadas.Count == 0)
            throw new InvalidOperationException("No se pudo crear ninguna vacuna. Todos los lotes ya existen");

        foreach (var (_, vacuna) in creadas)
            vacunas.Add(vacuna);

        _repoVacuna.GuardarTodas(vacunas);
        return (creadas.Count, creadas[^1].Lote);
    }

    public ResultadoOperacion AplicarVacuna(string loteVacuna, string potreroId, string nombreRes)
    {
        var vacunas = _repoVacuna.ObtenerTodas();
        var vacuna = vacunas.FirstOrDefault(v => v.Lote.Equals(loteVacuna, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Vacuna con lote '{loteVacuna}' no encontrada");

        var potreros = _repoPotrero.ObtenerTodos();
        var potrero = potreros.FirstOrDefault(p => p.Identificacion.Valor.Equals(potreroId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Potrero '{potreroId}' no encontrado");
        var res = potrero.BuscarRes(nombreRes)
            ?? throw new InvalidOperationException($"Res '{nombreRes}' no encontrada");

        // La regla de límites por subtipo vive en el dominio (Res.AplicarVacuna), no se re-implementa aquí.
        res.AplicarVacuna(vacuna);
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

        return ResultadoOperacion.Ok(
            $"Vacuna '{vacuna.Nombre}' aplicada a '{nombreRes}' correctamente. Datos válidos. Guardado exitoso en BD.{mensajeEsquema}");
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
