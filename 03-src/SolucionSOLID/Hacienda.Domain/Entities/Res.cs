using Hacienda.Domain.Enums;
using Hacienda.Domain.Events;
using Hacienda.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hacienda.Domain.Entities;

/// <summary>
/// Raíz del agregado ganadero. Encapsulamiento real:
/// setters privados, colecciones de solo lectura y mutación por métodos con regla.
/// </summary>
public abstract class Res
{
    public Guid Id { get; }
    public string Nombre { get; }
    public uint Peso { get; private set; }
    public ushort Edad { get; private set; }
    public Chip? Chip { get; private set; }

    private readonly List<Vacuna> _vacunasAplicadas = new();
    public IReadOnlyList<Vacuna> VacunasAplicadas => _vacunasAplicadas.AsReadOnly();

    protected Res(Guid id, string nombre, uint peso, ushort edad)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador de la res no puede ser vacío", nameof(id));

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la res no puede estar vacío", nameof(nombre));

        if (peso == 0)
            throw new ArgumentException("El peso de la res debe ser mayor a 0", nameof(peso));

        if (edad == 0)
            throw new ArgumentException("La edad de la res debe ser mayor a 0", nameof(edad));

        var configuracion = CatalogoRes.Obtener(Tipo);
        if (!configuracion.RangoEdad.Contiene(edad))
            throw new InvalidOperationException(
                $"La edad {edad} no es válida para {Tipo}. Rango: {configuracion.RangoEdad}");

        Id = id;
        Nombre = nombre.Trim();
        Peso = peso;
        Edad = edad;
    }

    public abstract TipoRes Tipo { get; }
    public byte MaxVacunasBacterianas => CatalogoRes.Obtener(Tipo).MaxVacunasBacterianas;
    public byte MaxVacunasVivas => CatalogoRes.Obtener(Tipo).MaxVacunasVivas;
    public ushort PesoMinimo => CatalogoRes.Obtener(Tipo).PesoMinimo;
    public ushort PesoRecomendadoVenta => CatalogoRes.Obtener(Tipo).PesoRecomendadoVenta;

    /// <summary>Regla de edad del subtipo, declarada como dato (sin condiciones quemadas).</summary>
    public RangoEdad RangoEdad => CatalogoRes.Obtener(Tipo).RangoEdad;

    public bool EsEdadValida(ushort edad) => RangoEdad.Contiene(edad);

    public virtual bool EsquemaVacunacionCompleto()
    {
        int bac = _vacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Bacteriana);
        int viv = _vacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Viva);
        return bac >= MaxVacunasBacterianas && viv >= MaxVacunasVivas;
    }

    /// <summary>
    /// Registra la ganancia de peso de la res (mutación encapsulada; no hay setter público).
    /// </summary>
    public void Alimentar(uint pesoGanado)
    {
        if (pesoGanado == 0)
            throw new ArgumentOutOfRangeException(nameof(pesoGanado), pesoGanado, "El peso ganado debe ser mayor a 0");

        Peso += pesoGanado;
    }

    /// <summary>
    /// Instala el chip en la res (mutación encapsulada). La regla de negocio
    /// "una res tiene a lo sumo un chip" se valida aquí para proteger el agregado.
    /// </summary>
    public void InstalarChip(Chip chip)
    {
        if (chip == null)
            throw new ArgumentNullException(nameof(chip));

        if (Chip != null)
            throw new InvalidOperationException($"La res '{Nombre}' ya tiene un chip instalado ({Chip.NumeroSerie})");

        Chip = chip;
    }

    /// <summary>
    /// Aplica una vacuna exigiendo los límites del subtipo (regla del dominio,
    /// no del servicio). Mensajes idénticos a la implementación anterior.
    /// </summary>
    public void AplicarVacuna(Vacuna vacuna)
    {
        if (vacuna == null)
            throw new ArgumentNullException(nameof(vacuna));

        if (_vacunasAplicadas.Any(v => v.Lote.Equals(vacuna.Lote, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(
                $"La vacuna lote '{vacuna.Lote}' ya fue aplicada a '{Nombre}'");

        int bacActual = _vacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Bacteriana);
        int vivActual = _vacunasAplicadas.Count(v => v.Categoria == VacunaCategoria.Viva);

        if (vacuna.Categoria == VacunaCategoria.Bacteriana && bacActual >= MaxVacunasBacterianas)
            throw new InvalidOperationException(
                $"No se pueden aplicar más bacterianas a '{Nombre}' (máximo {MaxVacunasBacterianas})");

        if (vacuna.Categoria == VacunaCategoria.Viva && vivActual >= MaxVacunasVivas)
            throw new InvalidOperationException(
                $"No se pueden aplicar más vivas a '{Nombre}' (máximo {MaxVacunasVivas})");

        _vacunasAplicadas.Add(vacuna);
    }

    /// <summary>Registro mecánico para rehidratación desde persistencia (sin reglas).</summary>
    public void RegistrarVacunaEnHistorial(Vacuna vacuna) => _vacunasAplicadas.Add(vacuna);

    public abstract string Serializar();
}
