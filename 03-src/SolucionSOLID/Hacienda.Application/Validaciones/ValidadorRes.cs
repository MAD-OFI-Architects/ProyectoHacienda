using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Validaciones;

public class ValidadorRes : IValidarRes
{
    public ValidationResult Validar(Res res)
    {
        if (res == null) return ValidationResult.Fallo("Res no puede ser null");

        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(res.Nombre)) errores.Add("Nombre vacío");
        if (res.Peso == 0) errores.Add("Peso debe ser mayor a 0");
        return errores.Count == 0 ? ValidationResult.Exito() : ValidationResult.Fallo(errores.ToArray());
    }
}