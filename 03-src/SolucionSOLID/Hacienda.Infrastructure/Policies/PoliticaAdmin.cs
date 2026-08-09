using Hacienda.Application.Interfaces;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Results;

namespace Hacienda.Infrastructure.Policies;

public class PoliticaAdmin : IPoliticaPermisos
{
    public RolUsuario Rol => RolUsuario.Admin;

    public ResultadoAutorizacion Evaluar(string operacion)
        => ResultadoAutorizacion.Concedido(operacion);
}