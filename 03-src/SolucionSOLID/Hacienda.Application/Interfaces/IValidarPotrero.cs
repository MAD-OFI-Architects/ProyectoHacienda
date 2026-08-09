using Hacienda.Domain.Entities;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Interfaces;

public interface IValidarPotrero
{
    ValidationResult Validar(Potrero potrero);
}