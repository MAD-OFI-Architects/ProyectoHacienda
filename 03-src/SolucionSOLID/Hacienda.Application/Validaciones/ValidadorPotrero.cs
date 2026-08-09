using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Validaciones;

public class ValidadorPotrero : IValidarPotrero
{
    public ValidationResult Validar(Potrero potrero)
    {
        if (potrero == null) return ValidationResult.Fallo("Potrero no puede ser null");

        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(potrero.Identificacion.Valor)) errores.Add("Identificación vacía");
        return errores.Count == 0 ? ValidationResult.Exito() : ValidationResult.Fallo(errores.ToArray());
    }
}