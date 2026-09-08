namespace Hacienda.Domain.Interfaces;

/// <summary>
/// Contrato mínimo de un ítem de venta (ISP: una sola operación).
/// Lo implementan Res y ProductoDerivado — el polimorfismo del multi-ítem (SC-1).
/// </summary>
public interface IVendible
{
    string Descripcion { get; }
}
