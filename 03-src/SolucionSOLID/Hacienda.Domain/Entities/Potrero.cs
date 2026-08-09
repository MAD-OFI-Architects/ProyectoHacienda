using Hacienda.Domain.Enums;
using Hacienda.Domain.ValueObjects;
using System.Collections.Generic;

namespace Hacienda.Domain.Entities;

public class Potrero
{
    public Guid Id { get; }
    public Identificacion Identificacion { get; }
    public TipoPotrero Tipo { get; }
    public List<Res> Reses { get; }

    private const ushort MAX_RESES = 150;

    public Potrero(Guid id, Identificacion identificacion, TipoPotrero tipo)
    {
        Id = id;
        Identificacion = identificacion;
        Tipo = tipo;
        Reses = new List<Res>();
    }

    public void AgregarRes(Res res)
    {
        if (Reses.Count >= MAX_RESES)
            throw new InvalidOperationException(
                $"El potrero '{Identificacion}' está lleno ({MAX_RESES} reses máximo)");
        Reses.Add(res);
    }

    public void RemoverRes(Res res)
        => Reses.Remove(res);

    public Res? BuscarRes(string nombre)
        => Reses.FirstOrDefault(r =>
            r.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

    public ushort CantidadReses => (ushort)Reses.Count;
    public bool EstaALaMitad => CantidadReses == MAX_RESES / 2;
    public bool EstaLleno => CantidadReses >= MAX_RESES;
}