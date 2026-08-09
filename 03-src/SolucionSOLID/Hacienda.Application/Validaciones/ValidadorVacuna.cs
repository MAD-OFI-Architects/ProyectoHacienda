using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Validaciones;

public class ValidadorVacuna : IValidarVacuna
{
    public ValidationResult Validar(Vacuna vacuna)
    {
        if (vacuna == null) return ValidationResult.Fallo("Vacuna no puede ser null");

        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(vacuna.Nombre)) errores.Add("Nombre vacío");
        if (string.IsNullOrWhiteSpace(vacuna.Lote)) errores.Add("Lote vacío");
        return errores.Count == 0 ? ValidationResult.Exito() : ValidationResult.Fallo(errores.ToArray());
    }
}