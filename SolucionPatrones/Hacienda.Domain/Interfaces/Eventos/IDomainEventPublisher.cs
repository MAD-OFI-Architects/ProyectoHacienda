namespace Hacienda.Domain.Interfaces;

public interface IDomainEventPublisher
{
    void Publicar<TEvento>(TEvento evento) where TEvento : IDomainEvent;
}