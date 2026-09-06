using Hacienda.Domain.Interfaces;

namespace Hacienda.Infrastructure.Events;

/// <summary>Puente de despacho: el orden lo da el registro en DI (consola primero — determinista).</summary>
public interface IManejadorDeEventos
{
    bool Maneja(Type tipoEvento);
    void Procesar(IDomainEvent evento);
}
