using Hacienda.Domain.Enums;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Interfaces;

public interface IPoliticaPermisos
{
    RolUsuario Rol { get; }
    ResultadoAutorizacion Evaluar(string operacion);
}