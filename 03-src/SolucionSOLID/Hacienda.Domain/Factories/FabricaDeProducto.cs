using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.ValueObjects;
using Hacienda.Domain.Interfaces;

namespace Hacienda.Domain.Factories;

/// <summary>
/// Creator + Template Method (SC-1, P-13): mismo idioma que FabricaDeRes/FabricaDeVacuna.
/// Agregar un derivado nuevo = 1 subclase de ProductoDerivado + 1 creator + 1 registro.
/// </summary>
public abstract class FabricaDeProducto
{
    protected readonly IGuidProvider GuidProvider;

    protected FabricaDeProducto(IGuidProvider guidProvider) => GuidProvider = guidProvider;

    public abstract Enums.TipoProducto TipoAtendido { get; }

    protected abstract ProductoDerivado Construir(Guid id, string nombre, Dinero precio, uint stock, uint stockMinimo);

    /// <summary>Rehidrata desde persistencia conservando Id/stock persistidos (sin reglas).</summary>
    public ProductoDerivado Rehidratar(Guid id, string nombre, Dinero precio, uint stock, uint stockMinimo)
        => Construir(id, nombre, precio, stock, stockMinimo);

    public ProductoDerivado Crear(string nombre, decimal precio, uint stock, uint stockMinimo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del producto no puede estar vacío", nameof(nombre));
        if (precio <= 0)
            throw new ArgumentException("El precio del producto debe ser mayor a 0", nameof(precio));

        return Construir(GuidProvider.Nuevo(), nombre, new Dinero(precio), stock, stockMinimo);
    }
}
