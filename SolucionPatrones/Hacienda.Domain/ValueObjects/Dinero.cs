namespace Hacienda.Domain.ValueObjects;

public sealed record Dinero
{
    public decimal Monto { get; }
    public string Moneda { get; }

    public Dinero(decimal monto, string moneda = "COP")
    {
        if (monto < 0)
            throw new ArgumentException("El monto no puede ser negativo", nameof(monto));
        if (string.IsNullOrWhiteSpace(moneda))
            throw new ArgumentException("La moneda es obligatoria", nameof(moneda));
        Monto = monto;
        Moneda = moneda;
    }

    public Dinero Sumar(Dinero otro)
    {
        if (Moneda != otro.Moneda)
            throw new InvalidOperationException($"No se pueden sumar monedas distintas: {Moneda} vs {otro.Moneda}");
        return new Dinero(Monto + otro.Monto, Moneda);
    }

    public override string ToString() => $"{Monto:N2} {Moneda}";
}