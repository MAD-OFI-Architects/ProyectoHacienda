using Hacienda.Domain.Enums;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Domain.Entities;

/// <summary>
/// Producto derivado del ganado (SC-1): lácteos, carne, piel.
/// Encapsula su stock con regla propia (descontar exige disponibilidad; aviso de mínimo).
/// </summary>
public abstract class ProductoDerivado : IVendible
{
    public Guid Id { get; }
    public string Nombre { get; }
    public Dinero Precio { get; }
    public uint Stock { get; private set; }
    public uint StockMinimo { get; }

    public abstract TipoProducto Tipo { get; }
    public abstract string Unidad { get; }

    public string Descripcion => $"{Nombre} ({Unidad})";

    protected ProductoDerivado(Guid id, string nombre, Dinero precio, uint stock, uint stockMinimo)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador del producto no puede ser vacío", nameof(id));
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del producto no puede estar vacío", nameof(nombre));
        if (precio is null)
            throw new ArgumentNullException(nameof(precio));
        if (precio.Monto <= 0)
            throw new ArgumentException("El precio del producto debe ser mayor a 0", nameof(precio));

        Id = id;
        Nombre = nombre.Trim();
        Precio = precio;
        Stock = stock;
        StockMinimo = stockMinimo;
    }

    public void DescontarStock(uint cantidad)
    {
        if (cantidad > Stock)
            throw new InvalidOperationException(
                $"Stock insuficiente de '{Nombre}' (disponible: {Stock}, requerido: {cantidad})");
        Stock -= cantidad;
    }

    public void ReponerStock(uint cantidad) => Stock += cantidad;

    public bool EnStockMinimo => Stock <= StockMinimo;
}
