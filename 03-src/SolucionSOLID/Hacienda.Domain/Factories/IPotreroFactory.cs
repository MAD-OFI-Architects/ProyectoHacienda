using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;

namespace Hacienda.Domain.Factories;

public interface IPotreroFactory
{
    Potrero Crear(string identificacion, TipoPotrero tipo);
}