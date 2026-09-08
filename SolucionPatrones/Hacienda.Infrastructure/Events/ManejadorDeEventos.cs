using Hacienda.Domain.Interfaces;

namespace Hacienda.Infrastructure.Events;

/// <summary>Base de los handlers Observer: el tipo que manejan es su contrato.</summary>
public abstract class ManejadorDeEventos<TEvento> : IManejadorDeEventos, IDomainEventHandler<TEvento>
    where TEvento : IDomainEvent
{
    public bool Maneja(Type tipoEvento) => typeof(TEvento).IsAssignableFrom(tipoEvento);

    public void Procesar(IDomainEvent evento) => Manejar((TEvento)evento);

    public abstract void Manejar(TEvento evento);
}
