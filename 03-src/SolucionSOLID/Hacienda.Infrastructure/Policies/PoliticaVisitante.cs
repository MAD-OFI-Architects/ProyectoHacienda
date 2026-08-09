using Hacienda.Application.Interfaces;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Results;

namespace Hacienda.Infrastructure.Policies;

public class PoliticaVisitante : IPoliticaPermisos
{
    public RolUsuario Rol => RolUsuario.Visitante;

    public ResultadoAutorizacion Evaluar(string operacion)
        => (operacion.Contains("Consultar") || operacion.Contains("Listar"))
            ? ResultadoAutorizacion.Concedido(operacion)
            : ResultadoAutorizacion.Denegado("Visitante: solo consultar");
}