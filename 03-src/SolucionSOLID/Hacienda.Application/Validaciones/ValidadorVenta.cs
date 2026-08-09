using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Validaciones;

public class ValidadorVenta : IValidarVenta
{
    public ValidationResult Validar(Venta venta)
    {
        if (venta == null) return ValidationResult.Fallo("Venta no puede ser null");

        var errores = new List<string>();
        if (venta.Res == null) errores.Add("Res de la venta no puede ser null");
        if (venta.Monto.Monto <= 0) errores.Add("Monto debe ser mayor a 0");
        return errores.Count == 0 ? ValidationResult.Exito() : ValidationResult.Fallo(errores.ToArray());
    }
}