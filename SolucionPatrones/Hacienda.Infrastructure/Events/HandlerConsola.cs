using Hacienda.Domain.Interfaces;

namespace Hacienda.Infrastructure.Events;

/// <summary>Observer #1: reproduce la salida de consola ACTUAL byte a byte (congelado).</summary>
public class HandlerConsola : ManejadorDeEventos<IDomainEvent>
{
    public override void Manejar(IDomainEvent evento)
        => Console.WriteLine($"[DOMINIO] {evento.GetType().Name}: {evento}");
}
