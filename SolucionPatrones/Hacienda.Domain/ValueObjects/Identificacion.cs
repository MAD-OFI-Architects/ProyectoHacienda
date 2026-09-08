namespace Hacienda.Domain.ValueObjects;

public sealed record Identificacion
{
    public string Valor { get; }

    public Identificacion(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("La identificación no puede ser vacía", nameof(valor));
        Valor = valor.Trim();
    }

    public override string ToString() => Valor;
}