using Hacienda.Domain.Entities;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Interfaces;

public interface IValidarVacuna
{
    ValidationResult Validar(Vacuna vacuna);
}