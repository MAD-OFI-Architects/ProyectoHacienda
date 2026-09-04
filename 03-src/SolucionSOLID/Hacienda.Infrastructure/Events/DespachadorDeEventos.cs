using Hacienda.Domain.Interfaces;

namespace Hacienda.Infrastructure.Events;

/// <summary>
/// Observer (P-06, E-07): implementa la interfaz EXISTENTE IDomainEventPublisher —
/// los publicadores no cambian. Despacha a los handlers en orden de registro (determinista).
/// </summary>
public class DespachadorDeEventos : IDomainEventPublisher
{
    private readonly IReadOnlyList<IManejadorDeEventos> _manejadores;

    public DespachadorDeEventos(IEnumerable<IManejadorDeEventos> manejadores)
        => _manejadores = manejadores.ToList();

    public void Publicar<TEvento>(TEvento evento) where TEvento : IDomainEvent
    {
        var tipo = evento.GetType();
        foreach (var manejador in _manejadores.Where(m => m.Maneja(tipo)))
            manejador.Procesar(evento);
    }
}
