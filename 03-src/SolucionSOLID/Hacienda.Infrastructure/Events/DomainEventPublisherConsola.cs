using Hacienda.Domain.Interfaces;

namespace Hacienda.Infrastructure.Events;

public class DomainEventPublisherConsola : IDomainEventPublisher
{
    public void Publicar<TEvento>(TEvento evento) where TEvento : IDomainEvent
    {
        Console.WriteLine($"[DOMINIO] {evento.GetType().Name}: {evento}");
    }
}