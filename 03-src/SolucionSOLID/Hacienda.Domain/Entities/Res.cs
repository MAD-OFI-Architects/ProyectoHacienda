using Hacienda.Domain.Enums;
using Hacienda.Domain.Events;
using Hacienda.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace Hacienda.Domain.Entities;

public abstract class Res
{
    public Guid Id { get; }
    public string Nombre { get; }
    public uint Peso { get; set; }
    public ushort Edad { get; set; }
    public List<Vacuna> VacunasAplicadas { get; }
    public IChip Chip { get; set; }

    protected Res(Guid id, string nombre, uint peso, ushort edad)
    {
        Id = id;
        Nombre = nombre;
        Peso = peso;
        Edad = edad;
        VacunasAplicadas = new List<Vacuna>();
    }

    public abstract TipoRes Tipo { get; }
    public abstract byte MaxVacunasBacterianas { get; }
    public abstract byte MaxVacunasVivas { get; }
    public abstract ushort PesoMinimo { get; }
    public abstract ushort PesoRecomendadoVenta { get; }

    public abstract bool EsEdadValida(ushort edad);

    public virtual bool EsquemaVacunacionCompleto()
    {
        int bac = VacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Bacteriana);
        int viv = VacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Viva);
        return bac >= MaxVacunasBacterianas && viv >= MaxVacunasVivas;
    }

    public abstract string Serializar();
}