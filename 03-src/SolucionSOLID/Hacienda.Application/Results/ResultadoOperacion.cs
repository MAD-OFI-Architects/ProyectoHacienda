namespace Hacienda.Application.Results;

public sealed record ResultadoOperacion
{
    public bool Exito { get; }
    public string Mensaje { get; }
    public IReadOnlyList<string> Errores { get; }

    private ResultadoOperacion(bool exito, string mensaje, IEnumerable<string>? errores = null)
    {
        Exito = exito;
        Mensaje = mensaje;
        Errores = errores?.ToArray() ?? Array.Empty<string>();
    }

    public static ResultadoOperacion Ok(string mensaje)
        => new(true, mensaje, Array.Empty<string>());

    public static ResultadoOperacion Fallo(string mensaje)
        => new(false, mensaje, new[] { mensaje });

    public static ResultadoOperacion Fallo(IEnumerable<string> errores)
        => new(false, string.Join("; ", errores), errores.ToArray());
}
