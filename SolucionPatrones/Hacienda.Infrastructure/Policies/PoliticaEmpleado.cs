using Hacienda.Application.Interfaces;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Results;

namespace Hacienda.Infrastructure.Policies;

public class PoliticaEmpleado : IPoliticaPermisos
{
    public RolUsuario Rol => RolUsuario.Empleado;

    public ResultadoAutorizacion Evaluar(string operacion)
        => operacion.Contains("Eliminar")
            ? ResultadoAutorizacion.Denegado("Empleado no puede eliminar")
            : ResultadoAutorizacion.Concedido(operacion);
}