namespace Hacienda.Domain.Results;

public sealed record ResultadoAutorizacion
{
    public bool Permitido { get; }
    public string Motivo { get; }

    private ResultadoAutorizacion(bool permitido, string motivo)
    {
        Permitido = permitido;
        Motivo = motivo;
    }

    public static ResultadoAutorizacion Concedido(string operacion)
        => new(true, $"Operación '{operacion}' autorizada");
    public static ResultadoAutorizacion Denegado(string motivo)
        => new(false, motivo);
}