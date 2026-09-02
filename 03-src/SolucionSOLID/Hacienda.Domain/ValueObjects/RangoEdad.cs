namespace Hacienda.Domain.ValueObjects;

/// <summary>
/// Value Object que representa la regla de edad de un subtipo de res.
/// La regla (Contiene) vive una sola vez aquí; cada subtipo declara su rango como DATO.
/// Reemplaza las comparaciones quemadas en Ternero/Cebon/Novillo.
/// </summary>
public readonly record struct RangoEdad
{
    public ushort? Minimo { get; }
    public ushort? Maximo { get; }

    private RangoEdad(ushort? minimo, ushort? maximo)
    {
        if (minimo.HasValue && maximo.HasValue && minimo.Value > maximo.Value)
            throw new ArgumentException("El límite inferior del rango no puede superar el superior.");

        Minimo = minimo;
        Maximo = maximo;
    }

    public static RangoEdad Hasta(ushort maximo) => new(null, maximo);
    public static RangoEdad Entre(ushort minimo, ushort maximo) => new(minimo, maximo);
    public static RangoEdad Desde(ushort minimo) => new(minimo, null);

    public bool Contiene(ushort edad) =>
        (!Minimo.HasValue || edad >= Minimo.Value) &&
        (!Maximo.HasValue || edad <= Maximo.Value);

    public override string ToString()
        => (Minimo.HasValue, Maximo.HasValue) switch
        {
            (true, true) => $"[{Minimo}..{Maximo}]",
            (true, false) => $"[{Minimo}..∞)",
            (false, true) => $"[0..{Maximo}]",
            _ => "[0..∞)"
        };
}
