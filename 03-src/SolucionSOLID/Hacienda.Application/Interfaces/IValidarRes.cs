using Hacienda.Domain.Entities;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Interfaces;

public interface IValidarRes
{
    ValidationResult Validar(Res res);
}