using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Entities;

/// <summary>Ítem de una venta multi-ítem (SC-1): lo vendible + cantidad + subtotal.</summary>
public sealed record VentaItem(IVendible Vendible, int Cantidad, decimal Monto);

/// <summary>Ítem histórico para rehidratación de ventas (solo descripción; no revive agregados).</summary>
public sealed record ItemVendibleRegistro(string Descripcion) : IVendible;
