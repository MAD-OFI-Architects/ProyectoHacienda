using Hacienda.Domain.Enums;
using Hacienda.Domain.Entities;
using Hacienda.Domain.Factories;
using Hacienda.Domain.Interfaces;
using System.Collections.Generic;

namespace Hacienda.Domain.Factories;

public class FabricaRes : IResFactory
{
    private readonly Dictionary<TipoRes, Func<string, uint, ushort, Res>> _creators;
    private readonly IGuidProvider _guidProvider;

    public FabricaRes(IGuidProvider guidProvider)
    {
        _guidProvider = guidProvider;
        _creators = new()
        {
            [TipoRes.Ternero] = (n, p, e) => new Ternero(_guidProvider.Nuevo(), n, p, e),
            [TipoRes.Novillo] = (n, p, e) => new Novillo(_guidProvider.Nuevo(), n, p, e),
            [TipoRes.Cebon]   = (n, p, e) => new Cebon(_guidProvider.Nuevo(), n, p, e),
        };
    }

    public Res Crear(TipoRes tipo, string nombre, uint peso, ushort edad)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre no puede ser vacío", nameof(nombre));

        if (!_creators.TryGetValue(tipo, out var creator))
            throw new ArgumentException($"Tipo de res no soportado: {tipo}");

        Res res = creator(nombre, peso, edad);

        if (!res.EsEdadValida(edad))
            throw new InvalidOperationException(
                $"La edad {edad} no es válida para {tipo}. Rango: {DescribirRango(res)}");

        return res;
    }

    private static string DescribirRango(Res res) => res.Tipo switch
    {
        TipoRes.Ternero => "0-12 meses",
        TipoRes.Cebon => "13-48 meses",
        TipoRes.Novillo => "49+ meses",
        _ => "desconocido"
    };
}