using System;

namespace Hacienda.Domain.ValueObjects;

public readonly record struct NumeroSerieChip
{
    public string Valor { get; }

    public NumeroSerieChip(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("El número de serie del chip no puede ser vacío", nameof(valor));
        
        if (valor.Length > 50)
            throw new ArgumentException("El número de serie no puede exceder 50 caracteres", nameof(valor));
        
        Valor = valor.Trim().ToUpperInvariant();
    }

    public static implicit operator string(NumeroSerieChip numeroSerie) => numeroSerie.Valor;
    public static explicit operator NumeroSerieChip(string valor) => new NumeroSerieChip(valor);
    
    public override string ToString() => Valor;
}