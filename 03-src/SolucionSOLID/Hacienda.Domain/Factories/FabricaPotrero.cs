using Hacienda.Domain.Enums;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Interfaces;
using Hacienda.Domain.ValueObjects;

namespace Hacienda.Domain.Factories;

public class FabricaPotrero : IPotreroFactory
{
    private readonly IGuidProvider _guidProvider;

    public FabricaPotrero(IGuidProvider guidProvider)
    {
        _guidProvider = guidProvider;
    }

    public Potrero Crear(string identificacion, TipoPotrero tipo)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
            throw new ArgumentException("La identificación no puede ser vacía", nameof(identificacion));

        return new Potrero(
            _guidProvider.Nuevo(),
            new Identificacion(identificacion),
            tipo);
    }
}