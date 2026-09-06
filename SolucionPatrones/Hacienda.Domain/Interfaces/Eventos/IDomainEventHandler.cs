namespace Hacienda.Domain.Interfaces;

/// <summary>
/// Observer (P-06, P-14): contrato de quien reacciona a un evento de dominio.
/// El publicador no conoce a sus suscriptores; el orden lo da el composition root.
/// </summary>
public interface IDomainEventHandler<in TEvento> where TEvento : IDomainEvent
{
    void Manejar(TEvento evento);
}
