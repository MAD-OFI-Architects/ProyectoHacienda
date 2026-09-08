using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Results;

namespace Hacienda.Application.Interfaces;

public interface IAutorizador
{
    ResultadoAutorizacion Autorizar(Usuario usuario, string operacion);
}