using Hacienda.Domain.Enums;
using Hacienda.Domain.Reglas;
using Hacienda.Domain.ValueObjects;
using System.Collections.Generic;

namespace Hacienda.Domain.Entities;

public class Potrero
{
    private readonly List<Res> _reses;

    public Guid Id { get; }
    public Identificacion Identificacion { get; }
    public TipoPotrero Tipo { get; }
    public IReadOnlyList<Res> Reses => _reses.AsReadOnly();

    public Potrero(Guid id, Identificacion identificacion, TipoPotrero tipo)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador del potrero no puede ser vacío", nameof(id));

        if (identificacion == null)
            throw new ArgumentNullException(nameof(identificacion));

        Id = id;
        Identificacion = identificacion;
        Tipo = tipo;
        _reses = new List<Res>();
    }

    public void AgregarRes(Res res)
    {
        if (res == null)
            throw new ArgumentNullException(nameof(res));

        if (_reses.Count >= ParametrosPotrero.CapacidadMaxima)
            throw new InvalidOperationException(
                $"El potrero '{Identificacion}' está lleno ({ParametrosPotrero.CapacidadMaxima} reses máximo)");

        if (_reses.Any(r => r.Nombre.Equals(res.Nombre, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Ya existe una res '{res.Nombre}' en el potrero '{Identificacion}'");

        _reses.Add(res);
    }

    public void RemoverRes(Res res)
    {
        if (res == null)
            throw new ArgumentNullException(nameof(res));

        _reses.Remove(res);
    }

    public Res? BuscarRes(string nombre)
        => _reses.FirstOrDefault(r =>
            r.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

    public ushort CantidadReses => (ushort)_reses.Count;
    public bool EstaALaMitad => CantidadReses == ParametrosPotrero.CapacidadMaxima / 2;
    public bool EstaLleno => CantidadReses >= ParametrosPotrero.CapacidadMaxima;
}