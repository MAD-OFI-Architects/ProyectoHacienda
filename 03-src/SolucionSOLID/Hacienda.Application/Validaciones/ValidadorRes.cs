using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Validaciones;

public class ValidadorRes : IValidarRes
{
    public ValidationResult Validar(Res res)
    {
        if (res == null) return ValidationResult.Fallo("La res no puede ser null");

        var errores = new List<string>();
        if (res.Id == Guid.Empty) errores.Add("El identificador de la res no puede ser vacío");
        if (string.IsNullOrWhiteSpace(res.Nombre)) errores.Add("El nombre de la res no puede estar vacío");
        if (res.Peso == 0) errores.Add("El peso de la res debe ser mayor a 0");
        if (res.Edad == 0) errores.Add("La edad de la res debe ser mayor a 0");

        return errores.Count == 0 ? ValidationResult.Exito() : ValidationResult.Fallo(errores);
    }
}
