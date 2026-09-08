using Hacienda.Application.Interfaces;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Enums;
using Hacienda.Domain.Results;
using System.Collections.Generic;

namespace Hacienda.Application.Services;

public class AutorizadorRbca : IAutorizador
{
    private readonly Dictionary<RolUsuario, IPoliticaPermisos> _politicas;

    public AutorizadorRbca(IEnumerable<IPoliticaPermisos> politicas)
    {
        _politicas = politicas.ToDictionary(p => p.Rol);
    }

    public ResultadoAutorizacion Autorizar(Usuario usuario, string operacion)
    {
        if (usuario == null)
            return ResultadoAutorizacion.Denegado("Usuario no autenticado");

        if (_politicas.TryGetValue(usuario.Rol, out var politica))
            return politica.Evaluar(operacion);

        return ResultadoAutorizacion.Denegado($"Rol no configurado: {usuario.Rol}");
    }
}