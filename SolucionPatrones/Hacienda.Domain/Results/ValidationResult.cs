namespace Hacienda.Domain.Results;

public sealed record ValidationResult
{
    public bool EsValido { get; }
    public IReadOnlyList<string> Errores { get; }

    private ValidationResult(bool esValido, IReadOnlyList<string> errores)
    {
        EsValido = esValido;
        Errores = errores;
    }

    public static ValidationResult Exito() => new(true, Array.Empty<string>());
    public static ValidationResult Fallo(params string[] errores) => new(false, errores);
    public static ValidationResult Fallo(IEnumerable<string> errores) => new(false, errores.ToArray());
}